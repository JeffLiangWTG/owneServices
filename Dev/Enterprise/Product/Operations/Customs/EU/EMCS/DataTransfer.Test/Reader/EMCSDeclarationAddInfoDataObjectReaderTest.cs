using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSDeclarationAddInfoDataObjectReaderTest : OrganizationAddressTestHelper
	{
		[TestDate(2018, 3, 19, 12, 12, 12)]
		public void TestReadIntoBusinessObject_AddInfoReader_FixJourneyTime()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var jobData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MessagingApplicationCode = new CodeDescriptionPair
					{
						Code = "EMC"
					},
					DataContext = DataContextFactory.New()
				};
				jobData.SetAddInfoCollection(() => new List<AddInfo>()
					{
						new AddInfo
						{
							Key = "JourneyTime",
							Value = "2H"
						}
					});
				jobData.DataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var reader = new EMCSDeclarationDataObjectReader(jobData, Logger, Factory);
				var emcs = (EMCSJobDeclaration)reader.ReadIntoBusinessObject();

				CombineAssertions(() =>
				{
					AssertEquals("JourneyTimeNumericPart", 2, emcs.JourneyTimeNumericPart);
					AssertEquals("JourneyTimeFormatPart", JourneyTimeUnitList.Codes.Hours, emcs.JourneyTimeFormatPart);
				});
				ErrorReporter.Clear();

				jobData.AddInfoCollection[0].Value = " 3H";
				reader = new EMCSDeclarationDataObjectReader(jobData, Logger, Factory);
				emcs = (EMCSJobDeclaration)reader.ReadIntoBusinessObject();

				CombineAssertions(() =>
				{
					AssertEquals("JourneyTimeNumericPart", 3, emcs.JourneyTimeNumericPart);
					AssertEquals("JourneyTimeFormatPart", JourneyTimeUnitList.Codes.Hours, emcs.JourneyTimeFormatPart);
				});
				ErrorReporter.Clear();
			}
		}
	}
}
