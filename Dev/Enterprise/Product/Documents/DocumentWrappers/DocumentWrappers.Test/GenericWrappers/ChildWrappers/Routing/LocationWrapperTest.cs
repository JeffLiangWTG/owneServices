using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(LocationWrapper))]
	sealed class LocationWrapperTest : Base.Testing.GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			LocationWrapper wrapperEmpty = new LocationWrapper("", Factory);
			AssertEquals("wrapperEmpty.Country.Code", ZString.Empty, wrapperEmpty.Country.Code);
			AssertEquals("wrapperEmpty.IATACode", ZString.Empty, wrapperEmpty.IATACode);
			AssertEquals("wrapperEmpty.PortName", ZString.Empty, wrapperEmpty.PortName);
			AssertEquals("wrapperEmpty.UNLOCO", ZString.Empty, wrapperEmpty.UNLOCO);
			AssertEquals("wrapperEmpty.UNLOCOAndPortName", ZString.Empty, wrapperEmpty.UNLOCOAndPortName);
			AssertEquals("wrapperEmpty.State", ZString.Empty, wrapperEmpty.State);
		}

		public void TestWrapperMappingJohannesburg()
		{
			RefUNLOCO zAJNB = new RefUNLOCO.Loader(Factory).Load("ZAJNB");
			LocationWrapper wrapperJohannesburg = new LocationWrapper(zAJNB.RL_Code, Factory);
			AssertEquals("wrapperJohannesburg.Country.Code", zAJNB.RL_Code.Left(2), wrapperJohannesburg.Country.Code);
			AssertEquals("wrapperJohannesburg.IATACode", zAJNB.RL_IATA, wrapperJohannesburg.IATACode);
			AssertEquals("wrapperJohannesburg.PortName", zAJNB.RL_PortName, wrapperJohannesburg.PortName);
			AssertEquals("wrapperJohannesburg.UNLOCO", zAJNB.RL_Code, wrapperJohannesburg.UNLOCO);
			AssertEquals("wrapperJohannesburg.UNLOCOAndPortName", zAJNB.RL_Code + " - " + zAJNB.RL_PortName, wrapperJohannesburg.UNLOCOAndPortName);
			AssertEquals("wrapperJohannesburg.State", ZString.Empty, wrapperJohannesburg.State);
		}

		public void TestWrapperMappingTokyo()
		{
			RefUNLOCO jPTYO = new RefUNLOCO.Loader(Factory).Load("JPTYO");
			LocationWrapper wrapperTokyo = new LocationWrapper(jPTYO.RL_Code, Factory);
			AssertEquals("wrapperTokyo.Country.Code", jPTYO.RL_Code.Left(2), wrapperTokyo.Country.Code);
			AssertEquals("wrapperTokyo.IATACode", jPTYO.RL_IATA, wrapperTokyo.IATACode);
			AssertEquals("wrapperTokyo.PortName", jPTYO.RL_PortName, wrapperTokyo.PortName);
			AssertEquals("wrapperTokyo.UNLOCO", jPTYO.RL_Code, wrapperTokyo.UNLOCO);
			AssertEquals("wrapperTokyo.UNLOCOAndPortName", jPTYO.RL_Code + " - " + jPTYO.RL_PortName, wrapperTokyo.UNLOCOAndPortName);
			AssertEquals("wrapperTokyo.State", jPTYO.CountryStates.RW_Description, wrapperTokyo.State);
		}

		public void TestWrapperMappingInvalid()
		{
			LocationWrapper wrapperInvalid = new LocationWrapper("ZXZXZX", Factory);
			AssertEquals("wrapperInvalid.Country.Code", "ZX", wrapperInvalid.Country.Code);
			AssertEquals("wrapperInvalid.IATACode", "ZXZ", wrapperInvalid.IATACode);
			AssertEquals("wrapperInvalid.PortName", "ZXZXZX", wrapperInvalid.PortName);
			AssertEquals("wrapperInvalid.UNLOCO", "ZXZXZX", wrapperInvalid.UNLOCO);
			AssertEquals("wrapperInvalid.UNLOCOAndPortName", "ZXZXZX", wrapperInvalid.UNLOCOAndPortName);
			AssertEquals("wrapperInvalid.State", "", wrapperInvalid.State);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Location                            (Default Field: UNLOCOAndPortName)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Country                                 Country
IATACode                                String
PortName                                String
State                                   String
UNLOCO                                  String
UNLOCOAndPortName                       String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Country : JP - Japan
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			RefUNLOCO jPTYO = new RefUNLOCO.Loader(Factory).Load("JPTYO");
			return new LocationWrapper(jPTYO.RL_Code, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new LocationWrapper(null, Factory);
		}
	}
}
