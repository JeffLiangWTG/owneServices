using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.FR.Business.Interfaces;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(DeltaGAutoSendCustomsMessageProcessor))]
	public abstract class DeltaGAutoSendCustomsMessageProcessorTest<T> : AutoSendCustomsMessageProcessorTest<T> where T : IAutoSendCustomsMessageRule, new()
	{
		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			var frDeclaration = (JobDeclaration)declaration;
			frDeclaration.CustomsEntryInstructions[0].CEI_SubStyle = "";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.SetupAccount(OrgCusAccountCodeList.Codes.DGI, OrgCusAccountDeltaGTypeList.Codes.G1, "DGI001", ZString.Empty, ZString.Empty, "749AF8CE");
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "12345678", Core.Constants.CountryCodes.France);
			frDeclaration.JE_OH_Importer = importer.PK;

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			frDeclaration.JE_OH_Supplier = supplier.PK;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.Siret, "12345678", Core.Constants.CountryCodes.France);
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345678", Core.Constants.CountryCodes.France);
			declarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.BrokerageRegistration, "12345678", Core.Constants.CountryCodes.France);
			frDeclaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			frDeclaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
			frDeclaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			frDeclaration.JE_MasterBill = "TESTBLL";
			frDeclaration.JE_DeltaMode = OrgCusAccountDeltaGTypeList.Codes.G1;
			frDeclaration.JE_CustomsProfile = "DGI001";
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			return new DeltaGAutoSendCustomsMessageProcessorForTest((JobDeclaration)declaration, OriginalEntryStatusForProcessing);
		}
	}

	public class DeltaGAutoSendCustomsMessageProcessorForTest : DeltaGAutoSendCustomsMessageProcessor
	{
		public DeltaGAutoSendCustomsMessageProcessorForTest(JobDeclaration declaration, ZString entryStatus) : base(declaration)
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
