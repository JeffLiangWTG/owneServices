using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.DataTransfer.Universal.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.IE.DataTransfer.Universal.Testing
{
	sealed class JobDeclarationDataObjectReaderTest : JobDeclarationDataObjectReaderAbstractTest<JobDeclaration, Reader.JobDeclarationDataObjectReader>
	{
		#region PopulateApplicationCode

		public void TestPopulateApplicationCode()
		{
			foreach (var testCase in GetTestCases())
			{
				TestPopulateApplicationCode(testCase);
			}
		}

		void TestPopulateApplicationCode((string InterfaceSubmissionType, string InputMessageType, string InputApplicationCode, string ExpectedApplicationCode, HashSet<string> UnexpectedApplicationCodes, string Message) testCase)
		{
			if (testCase.ExpectedApplicationCode == null == (testCase.UnexpectedApplicationCodes == null))
			{
				throw new ArgumentException($"{nameof(testCase.ExpectedApplicationCode)} and {nameof(testCase.UnexpectedApplicationCodes)} cannot not be null/not null at the same time.");
			}
			var customsInterface = new LocalCountryCustomsInterface { SubmissionType = testCase.InterfaceSubmissionType };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = testCase.InputMessageType },
					MessagingApplicationCode = new CodeDescriptionPair() { Code = testCase.InputApplicationCode },
				};
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				if (testCase.ExpectedApplicationCode != null)
				{
					AssertEquals(testCase.Message, testCase.ExpectedApplicationCode, ((JobDeclaration)bizObj).JE_ApplicationCode);
				}
				else if (testCase.UnexpectedApplicationCodes != null)
				{
					AssertEquals(testCase.Message, false, testCase.UnexpectedApplicationCodes.Contains(((JobDeclaration)bizObj).JE_ApplicationCode));
				}
			}
		}

		static IEnumerable<(string InterfaceSubmissionType, string InputMessageType, string InputApplicationCode, string ExpectedApplicationCode, HashSet<string> UnexpectedApplicationCodes, string Message)> GetTestCases()
		{
			var blt = DeclarationApplicationCodeList.Codes.Builtin;
			var imp = JobMessageTypeList.Codes.Import;
			var v1 = ImportDeclarationApplicationCodeList.Codes.V1;

			yield return (blt, imp, v1, v1, null, "IMP&V1/V2, expecting ApplicationCode V1/V2.");

			var v2 = ImportDeclarationApplicationCodeList.Codes.V2;
			yield return (blt, imp, v2, v2, null, "IMP&V1/V2, expecting ApplicationCode V1/V2.");

			var bth = DeclarationApplicationCodeListForRegistry.Codes.BothBuiltInDefaulted;
			yield return (bth, imp, v1, v1, null, "IMP&V1/V2, expecting ApplicationCode V1/V2.");
			yield return (bth, imp, v2, v2, null, "IMP&V1/V2, expecting ApplicationCode V1/V2.");

			yield return (blt, imp, "XXX", v1, null, "IMP&non-V1/V2,ApplicationCode relies on registry LocalCountryCustomsInterface, V1 when BLT/BTH.");
			yield return (bth, imp, "XXX", v1, null, "IMP&non-V1/V2,ApplicationCode relies on registry LocalCountryCustomsInterface, V1 when BLT/BTH.");

			var inf = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			yield return (inf, imp, inf, inf, null, "IMP&non-V1/V2,ApplicationCode relies on registry LocalCountryCustomsInterface, INF when INF");
		}

		public void TestPopulateApplicationCodeIfExport()
		{
			var customsInterface = new LocalCountryCustomsInterface { SubmissionType = DeclarationApplicationCodeList.Codes.Builtin };
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Export },
					MessagingApplicationCode = new CodeDescriptionPair() { Code = ImportDeclarationApplicationCodeList.Codes.V1 },
				};
				BusinessObject bizObj = null;
				AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj));
				AssertNull(bizObj);
			}
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			currentCompany = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland);
			provider = new CustomsShipmentDataObjectReaderProvider();
			logger = new TestErrorLogger();
		}

		protected override void TearDown()
		{
			currentCompany.Dispose();
			base.TearDown();
		}
		IDisposable currentCompany;
		CustomsShipmentDataObjectReaderProvider provider;
	}
}
