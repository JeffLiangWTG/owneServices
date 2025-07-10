using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.TTCE;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class BRCMandatoryTreatmentAttributesResponseMessageProcessor : BRCResponseMessageProcessor
	{
		public BRCMandatoryTreatmentAttributesResponseMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("4b207723-a112-4ba5-b8d8-eae96b0edf3b", "Mandatory Treatment Attributes Response Message");

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.RTT };

		protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { EDIMessageSubTypeList.Codes.MandatoryTributaryTreatment };

		protected override BusinessObject GetLinkedObject(EDIMessage message) => GetLinkedObjectFromOutgoingMessage(message);

		protected override void ProcessResponseMessage(EDIMessage message)
		{
			if (message.EM_LinkedObject is JobDeclaration declaration)
			{
				var ttce = BRMessageHelper.DeserializeObject<RespostaObterTratamentosTributariosImportacaoDTO>(message.EM_MessageText);
				var ncm = ttce?.ncm;
				if (!string.IsNullOrEmpty(ncm) && ttce.codigoPais > 0)
				{
					var countryCode = BRRefCusMapper.MapCustomsCodeCountryToCW1Code(declaration.Factory, ttce.codigoPais.ToString());
					if (!countryCode.IsEmpty)
					{
						message.EM_ApplicationReference = $"{ncm}|{countryCode}|{ttce.dataFatoGerador.Replace("-", string.Empty)}";
						var lines = declaration.Factory.Load<JobComInvoiceLine>(GetInvoiceLinesLoadQuery(declaration.JE_ClusterKey, ncm, countryCode));
						if (lines.Length > 0)
						{
							AddMandatoryDuimpTaxRegimes(lines, ttce);
							new ImportTaxTreatmentsOptionalMessageSender(declaration, ttce).SendMessage();
						}
						else
						{
							message.EM_Status = EDIMessage.Status.Failed;
							Logger.LogError($"Message #{message.EM_MessageNum}: Invoice Line with Tariff '{ncm}' and Origin Country '{countryCode}' not found.");
						}
					}
					else
					{
						message.EM_Status = EDIMessage.Status.Failed;
						Logger.LogError($"Message #{message.EM_MessageNum}: Code '{ttce.codigoPais}' has not Country mapped.");
					}
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
					Logger.LogError($"Message #{message.EM_MessageNum}: Message deserialization was failed.");
				}
			}
		}

		void AddMandatoryDuimpTaxRegimes(IEnumerable<JobComInvoiceLine> invoiceLines, RespostaObterTratamentosTributariosImportacaoDTO ttce)
		{
			var taxRegimes = ttce.tratamentosTributarios?.Where(w => w.fundamentoLegal?.tipo.Equals(Constants.LegalBasisType.Normal, System.StringComparison.OrdinalIgnoreCase) ?? false).ToArray() ?? [];
			if (taxRegimes.Length > 0)
			{
				invoiceLines.ForEach(l => l.Factory.AddFetchHint(typeof(DuimpTaxRegime),
					new ZQuery(CusSupportingInfoSchema.CSI_ParentID, l.PK).AddToFilter(CusSupportingInfoSchema.CSI_Type, CusSupportingInfoTypeList.Codes.DuimpTaxRegime)));

				foreach (var profile in TreatmentAttributesHelper.GetTariffProfilesFromTreatments(taxRegimes))
				{
					invoiceLines.ForEach(l => l.DuimpTaxRegimes.AddNew(profile));
				}
			}
		}

		ZQuery GetInvoiceLinesLoadQuery(int clusterKey, ZString tariff, ZString countryCode)
		{
			var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_CEI_Instruction, true);
			entryHeaderSubQuery.AddToFilter(BRCusEntryHeaderSchema.CH_ClusterKey, clusterKey);
			entryHeaderSubQuery.AddToFilter(BRCusEntryHeaderSchema.CH_AuthorityVersion, SQLComparisonOperator.NotEqual, new[] { string.Empty, "0" });

			var invoiceLineQuery = new ZDBOnlyQuery(typeof(JobComInvoiceLine));
			invoiceLineQuery.AddToFilter(JobComInvoiceLineSchema.JI_CEI, null);
			invoiceLineQuery.AddSubQuery(JobComInvoiceLineSchema.JI_CEI, entryHeaderSubQuery, JoinCondition.Or);

			var query = new ZQuery();
			query.AddToFilter(JobComInvoiceLineSchema.JI_ClusterKey, clusterKey);
			query.AddToFilter(JobComInvoiceLineSchema.JI_Tariff, tariff);
			query.AddToFilter(JobComInvoiceLineSchema.JI_CountryOfOrigin, countryCode);
			query.AddToFilter(invoiceLineQuery);

			var sql = query.FilterString.Replace(CusEntryHeaderSchema.Constants.TableName, BRCusEntryHeaderSchema.Constants.TableName);
			return new ZDBOnlyQuery(typeof(JobComInvoiceLine)).AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(query.Params), true);
		}
	}
}

