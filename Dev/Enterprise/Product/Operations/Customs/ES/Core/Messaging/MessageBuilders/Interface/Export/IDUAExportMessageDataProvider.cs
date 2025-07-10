using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IDUAExportMessageDataProvider : IExportMessageDataProviderCommon
	{
		#region Fields For BGM
		ZString MessageType { get; }

		#endregion

		#region Fields For CST

		ZString CustomsProcedureCategory1 { get; }
		ZString CustomsProcedureCategory2 { get; }
		ZString CustomsProcedureCategory3 { get; }
		ZString CustomsProcedureCategory4 { get; }
		ZString CustomsProcedureCategory5 { get; }

		#endregion

		#region Fields For LOC

		ZString CountryOfExport { get; }
		ZString CountryOfDestination { get; }
		ZString CustomsOfficeofExitCountryCode { get; }
		ZString CustomsOfficeofExit { get; }
		ZString LocationOfGoodsExamCustomsOffice { get; }
		ZString LocationOfGoodsExam { get; }
		ZString Warehouse { get; }

		#endregion

		#region Fields For DTM

		ZDateTime DateOfRecap { get; }

		#endregion

		#region Fields For GIS

		ZBool GoodsInContainerIndicator { get; }
		ZBool RMTIndicator { get; }

		#endregion

		#region Fields For EQD

		IReadOnlyCollection<ZString> CountryCodes { get; }

		#endregion

		#region Fields For SEL

		IReadOnlyCollection<ZString> SealCodes { get; }

		#endregion

		#region Fields For FTX

		ZString TextFunctionCode { get; }

		#endregion

		#region Fields For RFF

		ZString ReferenceNumber { get; }
		ZString SpecificCircumstancesIndicator { get; }

		#endregion

		#region Fields For TDT

		ITransportMediumInfoCommon BorderTransportMode { get; }
		ZString InternalTransportMode { get; }
		ZString TransportModeName { get; }

		#endregion

		#region Fields For NAD

		IDUAExportPartyProvider Exporter { get; }
		IDUAExportPartyProvider Receiver { get; }

		#endregion

		#region Fields For TOD

		#region Fields For LOC

		ZString LocationId { get; }

		#endregion

		#endregion

		#region Fields For MOA

		ZBool IsDeclarationInEuros { get; }

		#endregion

		#region Fields For Goods

		IReadOnlyCollection<IDUAExportLine> Lines { get; }

		#endregion

		#region Fields For CNT

		ZInt TotalNumberOfPackageElements { get; }

		#endregion
	}

	public interface IDUAExportPartyProvider : IPartyProvider
	{
		ZString OrganizationCodeQualifier { get; }
	}

	public interface IDUAExportLine : IExportLineCommon
	{
		#region Fields For CST

		ZString GoodsCustomsProcedureCategory1 { get; }
		ZString GoodsCustomsProcedureCategory2 { get; }
		ZString GoodsCustomsProcedureCategory3 { get; }
		ZString GoodsCustomsProcedureCategory4 { get; }

		#endregion

		#region Fields For FTX

		ZString GoodsDescription { get; }
		IDUAExportSpecialConditions SpecialConditions { get; }

		#endregion

		#region Fields For LOC

		ZString CountryOfOrigin { get; }
		ZString StateOfOrigin { get; }

		#endregion

		#region Fields For MEA

		ZDecimal SupplementaryUnitsNumber { get; }
		ZString SupplementaryUnitsQualifier { get; }

		#endregion

		#region Fields For TDT

		ZString DangerousGoodsCode { get; }

		#endregion

		#region Fields For PAC

		IExternalPackagesInfoCommon ExternalPackages { get; }
		IInternalPackagesInfoCommon InternalPackages { get; }
		IVehiclePackagesInfoCommon VehiclePackages { get; }

		#endregion

		#region Fields For RFF

		ZString DocumentReferenceNumber { get; }
		ZString DocumentTypeCode { get; }

		#endregion

		IReadOnlyCollection<IDUAExportDocuments> Documents { get; }
	}

	public interface IDUAExportSpecialConditions
	{
		ZString Code1 { get; }
		ZString Code2 { get; }
		ZString Code3 { get; }
		ZString Code4 { get; }
		ZString Text { get; }
	}

	public interface IDUAExportDocuments : IExportDocumentCommon
	{
		ZDecimal Quantity { get; }
		ZString QtyUnit { get; }
	}
}
