using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal;
using RF409Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.RF409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class RF409MessageInterpreter : InboundMessageInterpreter<RF409Provider>
	{
		public RF409MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, RF409Provider provider) : base(message, provider)
		{
		}
		protected override string Summary => Res.GetString("D2F8B581-6710-40B6-B049-D232E196C2B6", "Deposit Refund Application Decision (RF409) has been received and linked to job {0}. Decision: Refund Application {1}.", relatedJob.JobNumber, GetDecisionResult(provider.RefundApplicationAccepted));

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.ApplicationReferenceID, provider.ApplicationReferenceId);
			yield return (CommonResStrings.ApplicationDecisionCodeType, provider.ApplicationDecisionCodeType);
			yield return (CommonResStrings.RefundApplicationAccepted, provider.RefundApplicationAccepted.ToString());
			yield return (Res.GetString("FDDB0EB9-D9E5-47A6-8D07-E4A289EFD7BF", "Signature"), provider.Signature);
			yield return (CommonResStrings.DecisionTakingCustomsAuthority, provider.DecisionTakingCustomsAuthority);
			yield return (Res.GetString("3CAC5339-3AFF-4489-988E-D9A97090047A", "Total Number of Documents"), provider.TotalNumberOfDocuments);
			yield return (Res.GetString("CA6C171D-ABB4-4E66-B361-2D261DBD4710", "Applicant"), provider.Applicant);
			yield return (Res.GetString("F5CA88AA-41D3-43B3-85C7-4DA3107B4796", "Representative"), provider.RepresentativeIdentification);
			yield return (CommonResStrings.Date, provider.Date);
			yield return (Res.GetString("1D1A3666-A31B-4124-B0A9-3A13DF766930", "Office of Department"), provider.OfficeOfDept);
			yield return (Res.GetString("D5628644-16B0-4A2A-9070-1EDFDB1D4D83", "Office of Responsibility"), provider.OfficeOfResponsibility);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("DFCED308-7899-4B3F-8730-39F2076E2ED9", "Legal Basis Code"), provider.LegalBasisCode);
			yield return (Res.GetString("A13D2851-B560-4192-B307-88D6A888B510", "Legal Basis Description"),
				RefCusCodeListTypes.GetCachedList(message.Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.LegalBasisCode, ZDateTime.Today).GetDescriptionFromCode(provider.LegalBasisCode));

			yield return (Res.GetString("61E09DCD-1FDC-4296-B9B2-8ADCC5ECCFF9", "Customs procedure (request for completion of formalities)"), provider.CustomsProcedureCode);
			yield return (Res.GetString("3355AB68-6CED-4464-A694-1AE3D2288A46", "Amount of Duties to be Repaid or be Remitted"), provider.AmountOfDutiesToBeRemitted);
			yield return (Res.GetString("32879BD9-13FD-41E2-8854-4FCF6D549E2B", "Use or destination of goods"), provider.DestinationOfGoods);
			yield return (CommonResStrings.TimeLimitForCompletionOfFormalities, provider.TimeLimit);
			yield return (CommonResStrings.StatementOfTheDecisionTakingCustomsAuthority, provider.StatementOfTheDecision);
			yield return (CommonResStrings.DescriptionOfGrounds, provider.DescriptionOfGrounds);
		}

		protected override IEnumerable<(string summary, IEnumerable<string[]>)> GetAdditionalMessageWithFreeNumberOfColumns()
		{
			yield return (Res.GetString("E773EC76-06B1-42AB-8BB0-2B62A2F50FC5", "Goods Information"), GetGoodsInformtions(provider.GoodsInformations.ToArray()));
			yield return (Res.GetString("221ADCD9-EB59-4E0F-90FD-B83B74A59A71", "Type of import or export duty"), GetTypeOfImportOrExportDuties(provider.TypeOfDuties.ToArray()));
			yield return (Res.GetString("EDE0AB5A-2F96-45D6-B078-1826700729D7", "Attached Documents"), GetAttachedDocuments(provider.AttachedDocuments.ToArray()));
			yield return (CommonResStrings.GeneralRemarks, MessageInterpreterHelper.GetGeneralRemarks(provider.GeneralRemarks.ToArray()));
		}

		List<string[]> GetGoodsInformtions(GoodsInformationProvider[] goodsInformations)
		{
			var result = new List<string[]>
			{
				new string[]
				{
					CommonResStrings.Sequence,
					Res.GetString("8F440158-243D-40B4-9694-D41A87F4140E", "Customs Value Currency"),
					Res.GetString("A5B54C01-81CD-4E0F-8880-8EF8D9626943", "Customs Value Amount"),
				}
			};

			for (var i = 0; i < goodsInformations.Length; i++)
			{
				result.Add(new string[] { (i + 1).ToString(), goodsInformations[i].Currency, goodsInformations[i].Amount.ToString() });
			}
			return result;
		}

		List<string[]> GetTypeOfImportOrExportDuties(TypeOfDutyProvider[] typeOfDuties)
		{
			var result = new List<string[]>
			{
				new string[]
				{
					CommonResStrings.Sequence,
					Res.GetString("3399DCF9-3435-403B-B359-57683185CA59", "Union Code"),
					Res.GetString("57BFD3A4-8918-4A31-851A-5D404478131E", "National Code"),
				}
			};

			for (var i = 0; i < typeOfDuties.Length; i++)
			{
				result.Add(new string[] { (i + 1).ToString(), typeOfDuties[i].UnionCode, typeOfDuties[i].NationalCode.ToString() });
			}
			return result;
		}

		List<string[]> GetAttachedDocuments(Messaging.AttachedDocumentProvider[] attachedDocuments)
		{
			var result = new List<string[]>
			{
				new string[]
				{
					Res.GetString("EEF98149-9DB8-456B-82D1-AEEF3F9CFF25", "Document Type"),
					Res.GetString("B7A8DC86-8530-4341-A3C7-D40E4CC4EF8A", "Document Identifier"),
					Res.GetString("3017EED6-827F-4204-AFD1-F0C0B3FFB90F", "Document Date"),
				}
			};

			foreach (var attachedDocument in attachedDocuments)
			{
				result.Add(new string[] { attachedDocument.DocumentType, attachedDocument.DocumentIdentifier, attachedDocument.DocumentDate.ToString() });
			}
			return result;
		}

		string GetDecisionResult(bool value) => value ? Res.GetString("EBB24CD7-D522-41A3-9233-FBC500425A0A", "Accepted") : Res.GetString("BE0DEA0E-3B71-4693-8B28-6BC5CAA872A8", "Rejected");
	}
}
