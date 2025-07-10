using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class BRCCatalogZipFileResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCCatalogZipFileResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("D60EA6E1-375E-4A12-B6F4-D7F90B25F008", "Goods Catalog ZIP File Response");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => [MessageTypeList.Codes.CAT];

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => [EDIMessageSubTypeList.Codes.CatalogZipFile];

		protected override BusinessObject GetLinkedObject(EDIMessage message)
		{
			var productIntegration = BRMessageHelper.DeserializeObject<ProdutoIntegracaoDTO>(message.EM_MessageText);
			var rootCnpj = productIntegration?.cpfCnpjRaiz;
			if (productIntegration != null && productIntegration.codigo.HasValue && !rootCnpj.IsNullOrEmpty())
			{
				var owner = message.Factory.FindOrganizationByRootCNPJ(rootCnpj);
				if (owner != null)
				{
					return new CusGoodsCatalog.Loader(message.Factory).GetGoodsCatalogByAuthorityIdentifierAndOwner(productIntegration.codigo.ToString(), owner.PK);
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: Root CNPJ '{productIntegration.cpfCnpjRaiz}' does not have a matching organization. The message status was updated to 'FAL'.");
				}
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed, tag '{nameof(ProdutoIntegracaoDTO.codigo)}' or '{nameof(ProdutoIntegracaoDTO.cpfCnpjRaiz)}' not found. The message status was updated to 'FAL'.");
			}

			return null;
		}

		protected override bool FailIfLinkedObjectNotFound => false;

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			var productIntegration = BRMessageHelper.DeserializeObject<ProdutoIntegracaoDTO>(message.EM_MessageText);
			if (productIntegration != null && productIntegration.codigo.HasValue)
			{
				var identifier = productIntegration.codigo.ToString();
				if (message.EM_LinkedObject is CusGoodsCatalog goodsCatalog)
				{
					UpdateCusGoodsCatalog(goodsCatalog, identifier, productIntegration);
				}
				else if (message.Factory.FindOrganizationByRootCNPJ(productIntegration.cpfCnpjRaiz) is OrgHeader owner)
				{
					goodsCatalog = CreateCusGoodsCatalog(message, identifier, productIntegration, owner.PK);
					message.EM_LinkedObject = goodsCatalog;
				}
			}

			if (BRMessageHelper.GetOutgoingMessage(message) is EDIMessage outgoingMessage && outgoingMessage.EM_LinkedObject is OrgHeader ownerNew
				&& (message.Interchange?.ContainedMessages.Cast<EDIMessage>().Where(x => x.PK != message.PK).All(x => !BRMessageHelper.NotProcessedMessageStatuses.Contains(x.EM_Status)) ?? false))
			{ 
				ownerNew.Logs.AddNew(Events.AllDownloadCatalogResponsesProcessed, outgoingMessage.PK.ToString());
			}
		}

		CusGoodsCatalog CreateCusGoodsCatalog(EDIMessage message, ZString identifier, ProdutoIntegracaoDTO productIntegration, ZGuid ownerPK)
		{
			var errors = new ZStringBuilder();
			var type = GoodsCatalogTypeList.MapToCWCode(productIntegration.modalidade);
			if (type.IsEmpty)
			{
				errors.Append($"'{nameof(productIntegration.modalidade)}' tag is empty or invalid");
			}

			ZString description = productIntegration.denominacao;
			if (description.IsEmpty)
			{
				errors.Append($"'{nameof(productIntegration.denominacao)}' tag is empty");
			}

			CusGoodsCatalog goodsCatalog = null;

			var errorMessage = errors.ToStringWithDelimiterBetweenAppends(", ");
			if (errorMessage.Length == 0)
			{
				goodsCatalog = message.Factory.New<CusGoodsCatalog>();

				goodsCatalog.CGC_AuthorityIdentifier = identifier;
				goodsCatalog.CGC_Description = description;
				goodsCatalog.CGC_Type = type;
				goodsCatalog.CGC_Tariff = productIntegration.ncm;
				goodsCatalog.CGC_AuthorityVersion = productIntegration.versao;
				goodsCatalog.CGC_AuthorityStatus = GoodsCatalogStatusTypeList.MapToCWCode(productIntegration.situacao);
				goodsCatalog.CGC_OH_Owner = ownerPK;
				goodsCatalog.CGC_MessageStatus = BRMessageStatusList.Codes.Accepted;
				goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
				goodsCatalog.ComplementaryDescription = productIntegration.descricao;

				UpdateLocalPartNumbers(goodsCatalog, productIntegration);
				UpdateAttributes(goodsCatalog, productIntegration);

				goodsCatalog.Logs.AddNew(AutoEvents.CatalogAddedByDownload);
			}
			else
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.LogError($"Message #{message.EM_MessageNum}: {errorMessage}. The message status was updated to '{message.EM_Status}'.");
			}

			return goodsCatalog;
		}

		void UpdateCusGoodsCatalog(CusGoodsCatalog goodsCatalog, ZString identifier, ProdutoIntegracaoDTO productIntegration)
		{
			if (goodsCatalog.CGC_MessageStatus == BRMessageStatusList.Codes.Accepted)
			{
				goodsCatalog.SuspendResetStatusesUntilSaved();
				goodsCatalog.SuspendUpdateCustomStatusOnSavingUntilSaved();

				ZString description = productIntegration.denominacao;
				if (!description.IsEmpty && goodsCatalog.CGC_Description != description)
				{
					goodsCatalog.CGC_Description = description;
				}

				ZString version = productIntegration.versao;
				if (!version.IsEmpty && goodsCatalog.CGC_AuthorityVersion != version)
				{
					goodsCatalog.CGC_AuthorityVersion = version;
				}

				ZString complementaryDescription = productIntegration.descricao;
				if (!complementaryDescription.IsEmpty && goodsCatalog.ComplementaryDescription != complementaryDescription)
				{
					goodsCatalog.ComplementaryDescription = complementaryDescription;
				}

				UpdateLocalPartNumbers(goodsCatalog, productIntegration);
				UpdateAttributes(goodsCatalog, productIntegration);

				if (goodsCatalog.HasChanges)
				{
					goodsCatalog.CGC_CustomsStatus = CustomsPostedStatusList.Codes.Accepted;
					goodsCatalog.Logs.AddNew(Events.CatalogUpdatedByDownload);
					Logger.Log($"Catalog with Authority Identifier '{identifier}' already exists and was updated.");
				}
				else
				{
					goodsCatalog.Logs.AddNew(Events.CatalogNotUpdatedByDownload);
					Logger.Log($"Catalog with Authority Identifier '{identifier}' already exists and was not updated.");
				}
			}
			else
			{
				Logger.LogWarning($"Catalog with Authority Identifier '{identifier}' can only be updated when Message Status = 'ACC'");
			}
		}

		void UpdateLocalPartNumbers(CusGoodsCatalog goodsCatalog, ProdutoIntegracaoDTO productIntegration)
		{
			var localPartNumbersNeedToDelete = goodsCatalog.LocalPartNumbers.Cast<LocalPartNumber>().ToList();
			productIntegration.codigosInterno?.ForEach(partNumber =>
			{
				goodsCatalog.LocalPartNumbers.Find(partNumber).ForEach(x => localPartNumbersNeedToDelete.Remove(x));
				goodsCatalog.LocalPartNumbers.AddLocalPartNumberIfNotExists(partNumber);
			});

			var deletedLocalPartNumbers = localPartNumbersNeedToDelete.Select(x => x.CGI_Reference).ToArray();
			deletedLocalPartNumbers.ForEach(partNumber =>
			{
				goodsCatalog.Logs.AddNew(AutoEvents.ItemRemoved, new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description,
					$"Local Part Number {partNumber} was removed during the download because it was not contained in the latest downloaded JSON file."));
			});

			localPartNumbersNeedToDelete.ForEach(x => x.Delete());
			RemoveFromCusClassPartPivot(goodsCatalog, deletedLocalPartNumbers);
		}

		void RemoveFromCusClassPartPivot(CusGoodsCatalog goodsCatalog, ZString[] partNumbers)
		{
			if (partNumbers.Length > 0)
			{
				var partSubQuery = new ZDBOnlySubQuery(typeof(OrgSupplierPart), OrgSupplierPartSchema.PK);
				partSubQuery.AddToFilter(OrgSupplierPartSchema.OP_PartNum, partNumbers);

				var pivotQuery = new ZDBOnlyQuery(typeof(CusClassPartPivot));
				pivotQuery.AddToFilter(CusClassPartPivotSchema.CI_CGC_Catalog, goodsCatalog.PK);
				pivotQuery.AddSubQuery(CusClassPartPivotSchema.CI_OP, partSubQuery, JoinCondition.And);

				goodsCatalog.Factory.Load<CusClassPartPivot>(pivotQuery).ForEach(x => x.CI_CGC_Catalog = ZGuid.Empty);
			}
		}

		void UpdateAttributes(CusGoodsCatalog goodsCatalog, ProdutoIntegracaoDTO productIntegration)
		{
			var attributesNeedToDelete = goodsCatalog.Attributes.Cast<AttributeCusCodeData>().ToList();

			productIntegration.atributos?.ForEach(att =>
			{
				if (!string.IsNullOrEmpty(att.atributo))
				{
					var attribute = goodsCatalog.Attributes.AddNewOrUpdateExistingAttribute(att.atributo, att.valor);
					attributesNeedToDelete.Remove(attribute);
				}
			});

			productIntegration.atributosCompostos?.ForEach(att =>
			{
				if (!string.IsNullOrEmpty(att.atributo))
				{
					var attribute = goodsCatalog.Attributes.AddNewOrUpdateExistingAttribute(att.atributo, ZString.Empty);
					attributesNeedToDelete.Remove(attribute);

					att.valores?.ForEach(subAtt =>
					{
						if (!string.IsNullOrEmpty(subAtt.atributo))
						{
							var child = goodsCatalog.Attributes.AddNewOrUpdateExistingAttribute(subAtt.atributo, subAtt.valor);
							attributesNeedToDelete.Remove(child);
						}
					});
				}
			});

			productIntegration.atributosMultivalorados?.ForEach(att =>
			{
				if (!string.IsNullOrEmpty(att.atributo))
				{
					var attribute = goodsCatalog.Attributes.AddNewOrUpdateExistingAttribute(att.atributo, ZString.Empty);
					attribute.Content = string.Join(System.Environment.NewLine, att.valores);
					attributesNeedToDelete.Remove(attribute);
				}
			});

			attributesNeedToDelete.ForEach(x => x.Delete());
		}
	}
}
