using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteContextUtilsTest : TestCase
	{
		public void TestGetContextFromTransportModeImportExport()
		{
			AssertStmNoteContextsModule(StmNoteContextModule.F | StmNoteContextModule.I | StmNoteContextModule.E, Constants.GlobalModuleNamesConstants.Forwarding, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsModule(StmNoteContextModule.C | StmNoteContextModule.I, Constants.GlobalModuleNamesConstants.CFS, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsModule(StmNoteContextModule.O | StmNoteContextModule.I, Constants.GlobalModuleNamesConstants.Orders, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsModule(StmNoteContextModule.T, Constants.GlobalModuleNamesConstants.Transport, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsModule(StmNoteContextModule.W, Constants.GlobalModuleNamesConstants.Warehouse, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsModule(StmNoteContextModule.S, Constants.GlobalModuleNamesConstants.ShipsAgency, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsModule(StmNoteContextModule.D | StmNoteContextModule.E, Constants.GlobalModuleNamesConstants.CustomsDeclarations, ZString.Empty, ZString.Empty, ZString.Empty);

			AssertStmNoteContextsDirection(StmNoteContextDirection.I | StmNoteContextDirection.B, ZString.Empty, DirectionNamesConstants.Import, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsDirection(StmNoteContextDirection.E | StmNoteContextDirection.B, ZString.Empty, DirectionNamesConstants.Export, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsDirection(StmNoteContextDirection.D, ZString.Empty, DirectionNamesConstants.Domestic, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsDirection(StmNoteContextDirection.X, ZString.Empty, DirectionNamesConstants.CrossTrade, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsDirection(StmNoteContextDirection.F, ZString.Empty, DirectionNamesConstants.AllForwarding, ZString.Empty, ZString.Empty);
			AssertStmNoteContextsDirection(StmNoteContextDirection.O, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);

			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.S, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.TransportModes.Sea, ZString.Empty);
			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.I, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.TransportModes.Air, ZString.Empty);
			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.B, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.TransportModes.AirSea, ZString.Empty);
			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.B, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.TransportModes.SeaAir, ZString.Empty);
			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.R, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.TransportModes.Road, ZString.Empty);
			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.W, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.TransportModes.Rail, ZString.Empty);

			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.F, ZString.Empty, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.ContainerModes.FCL);
			AssertStmNoteContextsFreightMode(StmNoteContextFreightMode.L, ZString.Empty, ZString.Empty, ZString.Empty, Enterprise.Core.Constants.ContainerModes.LCL);
		}

		void AssertStmNoteContextsModule(StmNoteContextModule expectedModule, string module, string direction, string transportMode, string containerMode)
		{
			StmNoteContexts actualNoteContext = StmNoteContextUtils.GetContextFromTransportModeImportExport(module, direction, transportMode, containerMode);
			AssertEquals("testing StmNoteContexts.Module enum with Module inputs", expectedModule, actualNoteContext.Module);
			AssertEquals("testing StmNoteContexts.Module enum with Direction inputs", StmNoteContextDirection.O, actualNoteContext.Direction);
			AssertEquals("testing StmNoteContexts.Module enum with TransportMode and ContainerMode inputs", StmNoteContextFreightMode.Undefined, actualNoteContext.FreightMode);
		}

		void AssertStmNoteContextsDirection(StmNoteContextDirection expectedDirection, string module, string direction, string transportMode, string containerMode)
		{
			StmNoteContexts actualNoteContext = StmNoteContextUtils.GetContextFromTransportModeImportExport(module, direction, transportMode, containerMode);
			AssertEquals("testing StmNoteContexts.Module enum with Module inputs", StmNoteContextModule.Undefined, actualNoteContext.Module);
			AssertEquals("testing StmNoteContexts.Module enum with Direction inputs", expectedDirection, actualNoteContext.Direction);
			AssertEquals("testing StmNoteContexts.Module enum with TransportMode and ContainerMode inputs", StmNoteContextFreightMode.Undefined, actualNoteContext.FreightMode);
		}

		void AssertStmNoteContextsFreightMode(StmNoteContextFreightMode expectedFreightMode, string module, string direction, string transportMode, string containerMode)
		{
			StmNoteContexts actualNoteContext = StmNoteContextUtils.GetContextFromTransportModeImportExport(module, direction, transportMode, containerMode);
			AssertEquals("testing StmNoteContexts.Module enum with Module inputs", StmNoteContextModule.Undefined, actualNoteContext.Module);
			AssertEquals("testing StmNoteContexts.Module enum with Direction inputs", StmNoteContextDirection.O, actualNoteContext.Direction);
			AssertEquals("testing StmNoteContexts.Module enum with TransportMode and ContainerMode inputs", expectedFreightMode, actualNoteContext.FreightMode);
		}
	}
}
