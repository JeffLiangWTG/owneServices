using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(TrainingZoneRateDataType))]
	class TrainingZoneRateDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<TrainingZoneRateDataType>
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			zoneForTest1 = factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "AUEC"));
			zoneForTest1.FZ_ZoneType = EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training;
			zoneForTest2 = factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "NZDR"));
			zoneForTest2.FZ_ZoneType = EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training;
			factory.Save();
		}

		RefZoneHeader zoneForTest1;
		RefZoneHeader zoneForTest2;

		protected override TrainingZoneRateDataType GetNewDataType()
		{
			return new TrainingZoneRateDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "TrainingZoneRateRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			TrainingZoneRateCollection collection = new TrainingZoneRateCollection();
			TrainingZoneRate rate1 = collection.AddNew();
			rate1.ZonePK = zoneForTest1.PK;
			rate1.CurrencyCode = "JPY";
			rate1.RateAmount = 239;

			TrainingZoneRate rate2 = collection.AddNew();
			rate2.ZonePK = zoneForTest2.PK;
			rate2.CurrencyCode = "GBP";
			rate2.RateAmount = 39893;

			#region ByteArrayValue

			byte[] byteArrayValue = new byte[]
			{
				255,254,60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,
				0,102,0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,84,0,114,0,97,0,105,0,110,0,105,0,110,0,103,0,90,0,111,0,110,0,101,0,82,0,97,0,116,0,101,0,32,0,120,
				0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,
				0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,100,0,61,0,34,0,104,0,116,
				0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,34,0,62,0,60,
				0,84,0,114,0,97,0,105,0,110,0,105,0,110,0,103,0,90,0,111,0,110,0,101,0,82,0,97,0,116,0,101,0,62,0,60,0,90,0,111,0,110,0,101,0,62,0,99,0,52,0,51,0,50,0,102,0,98,0,57,0,101,0,45,0,98,
				0,100,0,99,0,51,0,45,0,52,0,100,0,51,0,98,0,45,0,98,0,51,0,53,0,99,0,45,0,49,0,97,0,56,0,98,0,56,0,50,0,56,0,50,0,99,0,50,0,54,0,99,0,60,0,47,0,90,0,111,0,110,0,101,0,62,
				0,60,0,82,0,97,0,116,0,101,0,65,0,109,0,111,0,117,0,110,0,116,0,62,0,50,0,51,0,57,0,60,0,47,0,82,0,97,0,116,0,101,0,65,0,109,0,111,0,117,0,110,0,116,0,62,0,60,0,67,0,117,0,114,0,114,
				0,101,0,110,0,99,0,121,0,62,0,74,0,80,0,89,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,99,0,121,0,62,0,60,0,47,0,84,0,114,0,97,0,105,0,110,0,105,0,110,0,103,0,90,0,111,0,110,0,101,
				0,82,0,97,0,116,0,101,0,62,0,60,0,84,0,114,0,97,0,105,0,110,0,105,0,110,0,103,0,90,0,111,0,110,0,101,0,82,0,97,0,116,0,101,0,62,0,60,0,90,0,111,0,110,0,101,0,62,0,100,0,102,0,50,0,55,
				0,56,0,100,0,48,0,99,0,45,0,53,0,98,0,48,0,57,0,45,0,52,0,101,0,98,0,99,0,45,0,98,0,49,0,51,0,53,0,45,0,57,0,100,0,48,0,49,0,53,0,49,0,101,0,57,0,99,0,53,0,100,0,49,0,60,
				0,47,0,90,0,111,0,110,0,101,0,62,0,60,0,82,0,97,0,116,0,101,0,65,0,109,0,111,0,117,0,110,0,116,0,62,0,51,0,57,0,56,0,57,0,51,0,60,0,47,0,82,0,97,0,116,0,101,0,65,0,109,0,111,0,117,
				0,110,0,116,0,62,0,60,0,67,0,117,0,114,0,114,0,101,0,110,0,99,0,121,0,62,0,71,0,66,0,80,0,60,0,47,0,67,0,117,0,114,0,114,0,101,0,110,0,99,0,121,0,62,0,60,0,47,0,84,0,114,0,97,0,105,
				0,110,0,105,0,110,0,103,0,90,0,111,0,110,0,101,0,82,0,97,0,116,0,101,0,62,0,60,0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,84,0,114,0,97,0,105,0,110,0,105,0,110,0,103,0,90,0,111,0,110,
				0,101,0,82,0,97,0,116,0,101,0,62,0
			};

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, byteArrayValue)
			};
		}

		#endregion
	}
}
