using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class InlandTransportCodeDescriptionPairListBuilderBaseTest<TBuilder> : TestCaseWithFactory
		where TBuilder : InlandTransportCodeDescriptionPairListBuilder
	{
		public void TestInlandTransportCodeList()
		{
			CombineAssertions(() =>
			{
				AssertInlandTransportCodeList("AIR", ExpectedTransportCodeList_AIR, ExpectedDefaultCode_AIR);
				AssertInlandTransportCodeList("FIX", ExpectedTransportCodeList_FIX, ExpectedDefaultCode_FIX);
				AssertInlandTransportCodeList("IWT", ExpectedTransportCodeList_IWT, ExpectedDefaultCode_IWT);
				AssertInlandTransportCodeList("OWN", ExpectedTransportCodeList_OWN, ExpectedDefaultCode_OWN);
				AssertInlandTransportCodeList("MAI", ExpectedTransportCodeList_MAI, ExpectedDefaultCode_MAI);
				AssertInlandTransportCodeList("RAI", ExpectedTransportCodeList_RAI, ExpectedDefaultCode_RAI);
				AssertInlandTransportCodeList("ROA", ExpectedTransportCodeList_ROA, ExpectedDefaultCode_ROA);
				AssertInlandTransportCodeList("SEA", ExpectedTransportCodeList_SEA, ExpectedDefaultCode_SEA);
				AssertInlandTransportCodeList(ZString.Empty, ExpectedTransportCodeList_Default, null);
			});
		}

		void AssertInlandTransportCodeList(string selectedTransportMode, string expectedTransportCodeList, string expectedDefaultCode)
		{
			declaration.TransportMeansDependencyForTest = EUCommonConstants.TransportModeSource.InlandTransportMode;
			declaration.JE_TransportModeInland = selectedTransportMode;
			var transportCodeList = GetNewBuilder(declaration).GetList();
			AssertEquals($"When Inland Transport Mode is {selectedTransportMode} and TransportMeansDependency is set to InlandTransportMode then TransportCodeList CodesAsString should be {expectedTransportCodeList}.", expectedTransportCodeList, transportCodeList.CodesAsString);
			AssertEquals($"When Inland Transport Mode is {selectedTransportMode} and TransportMeansDependency is set to InlandTransportMode then TransportCodeList DefaultCode should be {expectedDefaultCode}.", expectedDefaultCode, transportCodeList.DefaultCode);

			declaration.TransportMeansDependencyForTest = EUCommonConstants.TransportModeSource.TransportModeAtBorder;
			declaration.JE_TransportMode = selectedTransportMode;
			transportCodeList = GetNewBuilder(declaration).GetList();
			AssertEquals($"When Transport Mode is {selectedTransportMode} and TransportMeansDependency is set to TransportMode then TransportCodeList CodesAsString should be {expectedTransportCodeList}.", expectedTransportCodeList, transportCodeList.CodesAsString);
			AssertEquals($"When Transport Mode is {selectedTransportMode} and TransportMeansDependency is set to TransportMode then TransportCodeList DefaultCode should be {expectedDefaultCode}.", expectedDefaultCode, transportCodeList.DefaultCode);
		}

		protected virtual string ExpectedDefaultCode_AIR => "40";

		protected virtual string ExpectedDefaultCode_FIX => null;

		protected virtual string ExpectedDefaultCode_IWT => "81";

		protected virtual string ExpectedDefaultCode_OWN => null;

		protected virtual string ExpectedDefaultCode_MAI => null;

		protected virtual string ExpectedDefaultCode_RAI => "20";

		protected virtual string ExpectedDefaultCode_ROA => "30";

		protected virtual string ExpectedDefaultCode_SEA => "11";

		protected abstract string ExpectedTransportCodeList_AIR { get; }

		protected abstract string ExpectedTransportCodeList_FIX { get; }

		protected abstract string ExpectedTransportCodeList_IWT { get; }

		protected abstract string ExpectedTransportCodeList_OWN { get; }

		protected abstract string ExpectedTransportCodeList_MAI { get; }

		protected abstract string ExpectedTransportCodeList_RAI { get; }

		protected abstract string ExpectedTransportCodeList_ROA { get; }

		protected abstract string ExpectedTransportCodeList_SEA { get; }

		protected abstract string ExpectedTransportCodeList_Default { get; }

		protected abstract TBuilder GetNewBuilder(JobDeclaration declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclarationForTest>();
		}

		protected JobDeclarationForTest declaration;

		protected class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public EUCommonConstants.TransportModeSource TransportMeansDependencyForTest;

			protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => TransportMeansDependencyForTest;
		}
	}
}
