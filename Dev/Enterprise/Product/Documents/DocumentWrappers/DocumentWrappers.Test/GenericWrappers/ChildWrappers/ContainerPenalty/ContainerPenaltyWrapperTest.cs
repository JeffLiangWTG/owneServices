using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ContainerPenaltyWrapper))]
	sealed class ContainerPenaltyWrapperTest : GenericWrapperTest
	{
		public void TestContainerWrapper()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON001";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var exportPenalty = container.ExportPenalties.AddNew();

			var wrapper = new ContainerPenaltyWrapper(exportPenalty, Factory);
			AssertEquals(container, wrapper.Container.WrappedObject);
			AssertEquals("CON001", wrapper.Container.ContainerNo);
			AssertEquals("20GP", wrapper.Container.Type.Code);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var penalty = Factory.New<ContainerPenalty>();
			var emptyWrapper = new ContainerPenaltyWrapper(penalty, Factory);
			AssertEquals("emptyWrapper.Container.ContainerNo", ZString.Empty, emptyWrapper.Container.ContainerNo);
			AssertEquals("emptyWrapper.Container.Type.Code", ZString.Empty, emptyWrapper.Container.Type.Code);
			AssertEquals("emptyWrapper.AvailableDate", ZDateTime.Empty, emptyWrapper.AvailableDate);
			AssertEquals("emptyWrapper.FreeDays", ZInt.Zero, emptyWrapper.FreeDays);
			AssertEquals("emptyWrapper.FirstFreeDay", ZDateTime.Empty, emptyWrapper.FirstFreeDay);
			AssertEquals("emptyWrapper.LastFreeDay", ZDateTime.Empty, emptyWrapper.LastFreeDay);
			AssertEquals("emptyWrapper.PenaltyDays", ZInt.Zero, emptyWrapper.PenaltyDays);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new ContainerPenaltyWrapper(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
ContainerPenalty                            (Default Field: Container)
======================================================================
Name                                    Type
----------------------------------------------------------------------
Container                               Container
AvailableDate                           DateTime
FirstFreeDay                            DateTime
FreeDays                                Int
LastFreeDay                             DateTime
PenaltyDays                             Int
PenaltyType                             String
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Container : CON001
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CON001";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container.JC_FCLWharfGateIn = new ZDateTime(2022, 7, 18);

			var exportPenalty = container.ExportPenalties.AddNew();
			exportPenalty.CPY_PenaltyType = Constants.ContainerPenaltyPenaltyType.Codes.Detention;
			exportPenalty.FreeTimeAsDays = 5;
			exportPenalty.DurationAsDays = 10;
			return new ContainerPenaltyWrapper(exportPenalty, Factory);
		}
	}
}
