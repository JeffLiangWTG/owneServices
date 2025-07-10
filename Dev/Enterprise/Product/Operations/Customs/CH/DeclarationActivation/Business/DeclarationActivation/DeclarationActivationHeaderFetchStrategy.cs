using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

class DeclarationActivationHeaderFetchStrategy(DeclarationActivationHeader businessObject) : EnterpriseBusinessObjectFetchStrategy(businessObject)
{
	protected override void FetchForViewCore(TableColumn[] columns)
	{
		base.FetchForViewCore(columns);

		foreach (var column in columns)
		{
			switch (column.ColumnName)
			{
				case nameof(DeclarationActivationHeader.CXC_MovementReference):
				case nameof(DeclarationActivationHeader.CXC_ReferenceNumber):
					Factory.AddFetchHint(typeof(DeclarationActivationConsignment), CusExitConsignmentSchema.CXC_ClusterKey, BusinessObject.CXH_ClusterKey);
					break;
				case nameof(DeclarationActivationHeader.CER_AdditionalDeclarationType):
				case nameof(DeclarationActivationHeader.CER_Location):
				case nameof(DeclarationActivationHeader.CER_MessageStatus):
				case nameof(DeclarationActivationHeader.CER_OfficeOfExport):
				case nameof(DeclarationActivationHeader.CER_RN_NKTransportNationality):
				case nameof(DeclarationActivationHeader.CER_Status):
				case nameof(DeclarationActivationHeader.CER_TransportID):
				case nameof(DeclarationActivationHeader.CER_TransportMode):
				case nameof(DeclarationActivationHeader.CER_TransportType):
				case nameof(DeclarationActivationHeader.CER_Type):
				case nameof(DeclarationActivationHeader.CommunicationLanguage):
				case nameof(DeclarationActivationHeader.CustomsStatusDescription):
				case nameof(DeclarationActivationHeader.EdecOriginalTraderUID):
				case nameof(DeclarationActivationHeader.MessageStatusDescription):
				case nameof(DeclarationActivationHeader.NextProcedure):
				case nameof(DeclarationActivationHeader.NextProcedureDescription):
				case nameof(DeclarationActivationHeader.TypeDescription):
					Factory.AddFetchHint(typeof(DeclarationActivationReport), CusExitReportSchema.CER_ClusterKey, BusinessObject.CXH_ClusterKey);
					break;
				case nameof(DeclarationActivationHeader.ExporterCode):
				case nameof(DeclarationActivationHeader.ExporterName):
					Factory.AddFetchHint(typeof(OrgHeader), BusinessObject.CXH_OH_Exporter);
					break;
			}
		}
	}

	new DeclarationActivationHeader BusinessObject => (DeclarationActivationHeader)base.BusinessObject;
}
