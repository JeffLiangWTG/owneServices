using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DeltaIEAutoSendCustomsMessageProcessor))]
	public abstract class DeltaIEAutoSendCustomsMessageProcessorTest<T> : AutoSendCustomsMessageProcessorTest<T> where T : IAutoSendCustomsMessageRule, new()
	{
		protected override void PrepareDeclaration(BaseJobDeclaration frdeclaration)
		{
			var declaration = (JobDeclaration)frdeclaration;
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_FullName = "Importer";
			importer.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.WallisAndFutunaIslands;

			Factory.Save();

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.OH_FullName = "Declarant";
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.France);
			declarant.MainAddress.OA_Address1 = "177 Impasse Jane Poupelet";
			declarant.MainAddress.OA_Address2 = "Lescuretie";
			declarant.MainAddress.OA_PostCode = "24140";
			declarant.MainAddress.OA_City = "EYRAUD CREMPSE MAURENS";
			declarant.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			declaration.JE_GS_NKCusAgent = "CUS";
			declaration.CustomsOffices.RemoveAndDeleteAll();
			var supervisingOffice = declaration.CustomsOffices.AddNew();
			supervisingOffice.CY_Code = Enterprise.Customs.EU.Business.EuOfficeCodesTypes.Codes.CompetentAuthorityCountryOfDep;
			supervisingOffice.CY_Data = "FR000001";
			var customsOfficesOfDischarge = declaration.CustomsOffices.AddNew();
			customsOfficesOfDischarge.CY_Code = FrOfficeCodesTypes.Codes.OfficeOfDischarge;
			customsOfficesOfDischarge.CY_Data = "FR000002";
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			declaration.JE_EntryStyle = "IM";

			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_CustomsOffice = "FR230023";

			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.RemoveAndDeleteAll();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "98765432", Core.Constants.CountryCodes.France);
			declaration.JE_OA_Representative = representative.MainAddress.PK;
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			return new DeltaIEAutoSendCustomsMessageProcessorForTest((JobDeclaration)declaration, OriginalEntryStatusForProcessing);
		}
	}

	public class DeltaIEAutoSendCustomsMessageProcessorForTest : DeltaIEAutoSendCustomsMessageProcessor
	{
		public DeltaIEAutoSendCustomsMessageProcessorForTest(JobDeclaration declaration, ZString entryStatus) : base(declaration)
		{
			this.entryStatus = entryStatus;
		}

		protected override System.Collections.Generic.IEnumerable<Customs.Business.CusEntryHeader> GetEntryHeadersToSendCore()
		{
			var notifier = new SendsMessagesToCustomsShutterUpperer(false);
			var entries = Declaration.DoMerge(notifier) ? Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>() : Enumerable.Empty<Customs.Business.CusEntryHeader>();
			if (entries != null && entries.FirstOrDefault() != null)
			{
				entries.FirstOrDefault().CH_EntryStatus = entryStatus;
			}
			return entries;
		}

		readonly ZString entryStatus;

		public new IAutoSendCustomsMessageRule[] AutoSendCustomsMessageRules => base.AutoSendCustomsMessageRules;
	}
}
