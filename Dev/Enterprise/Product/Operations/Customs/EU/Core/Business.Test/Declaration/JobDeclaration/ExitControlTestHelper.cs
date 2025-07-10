using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public static class ExitControlTestHelper
{
	public static Integration.Customs.EUExitControl.ICusExitHeader CreateCusExitHeader(JobDeclaration declaration) => CreateCusExitHeader(declaration.Factory, declaration.PK, declaration.TablePrefix, declaration.JE_ClusterKey, declaration.JE_DeclarationReference);

	public static (Integration.Customs.EUExitControl.ICusExitHeader header, Integration.Customs.EUExitControl.ICusExitConsignment consignment, Integration.Customs.EUExitControl.ICusExitReport[] reports) CreateCusExitReportWithStatus(JobDeclaration declaration, ZString[] reportStatus)
	{
		var header = ExitControlTestHelper.CreateCusExitHeader(declaration);
		var factory = declaration.Factory;
		var consignment = ExitControlTestHelper.CreateCusExitConsignment(factory, header.PK, header.CXH_ClusterKey, "AAAAA");
		var reports = new List<Integration.Customs.EUExitControl.ICusExitReport>();
		foreach (var status in reportStatus)
		{
			var report = ExitControlTestHelper.CreateCusExitReport(factory, header.PK, header.CXH_ClusterKey, consignment.PK, ExitReportTypeList.Codes.Presentation, "IEDUB100");
			report.CER_Status = status;
			reports.Add(report);
		}
		return (header, consignment, reports.ToArray());
	}

	public static Integration.Customs.EUExitControl.ICusExitHeader CreateCusExitHeader(BusinessObjectFactory factory, ZGuid parentId, ZString parentTableCode, ZInt clusterKey, ZString jobReference)
	{
		var header = factory.New<Integration.Customs.EUExitControl.ICusExitHeader>();
		header.CXH_ParentID = parentId;
		header.CXH_ParentTableCode = parentTableCode;
		header.CXH_ClusterKey = clusterKey;
		header.CXH_JobReference = jobReference;
		return header;
	}

	public static Integration.Customs.EUExitControl.ICusExitConsignment CreateCusExitConsignment(BusinessObjectFactory factory, ZGuid headerPK, ZInt clusterKey, ZString movementReference)
	{
		var consignment = factory.New<Integration.Customs.EUExitControl.ICusExitConsignment>();
		consignment.CXC_CXH_Header = headerPK;
		consignment.CXC_ClusterKey = clusterKey;
		consignment.CXC_MovementReference = movementReference;
		return consignment;
	}

	public static Integration.Customs.EUExitControl.ICusExitReport CreateCusExitReport(BusinessObjectFactory factory, ZGuid headerPK, ZInt clusterKey, ZGuid consignmentPK, ZString type, ZString officeOfExit)
	{
		var report = factory.New<Integration.Customs.EUExitControl.ICusExitReport>();
		report.CER_CXH_Header = headerPK;
		report.CER_ClusterKey = clusterKey;
		report.CER_CXC_Consignment = consignmentPK;
		report.CER_Type = type;
		report.CER_OfficeOfExit = officeOfExit;
		report.CER_Behavior = ExitReportDiscrepancyTypeList.Codes.Discrepancies;
		return report;
	}
}
