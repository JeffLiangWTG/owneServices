using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class CusEntryHeaderTRCDetailsProvider : ITRCDetails
	{
		public CusEntryHeaderTRCDetailsProvider(CusEntryHeader cusEntryHeader)
		{
			this.cusEntryHeader = cusEntryHeader;
			this.jobDeclaration = cusEntryHeader.Declaration;
		}
		readonly CusEntryHeader cusEntryHeader;
		readonly JobDeclaration jobDeclaration;

		#region ITRCDetails Members
		BusinessObject ITRCDetails.BusinessObject => cusEntryHeader;

		ZString ITRCDetails.SourceType => nameof(DataContextType.CustomsDeclaration);

		ZString ITRCDetails.SourceID => jobDeclaration.JE_DeclarationReference;

		CommonContainer[] ITRCDetails.Containers => cusEntryHeader.Containers.Where(x => x.JobContainer != null).Select(x => x.JobContainer).ToArray();

		ZString ITRCDetails.PortOfOrigin => jobDeclaration.JE_RL_NKPortOfLoading;

		ZString ITRCDetails.PortOfDestination => jobDeclaration.JE_RL_NKPortOfArrival;

		RefUNLOCO ITRCDetails.OperationalPortImport => jobDeclaration.PortOfArrival;

		RefUNLOCO ITRCDetails.OperationalPortExport => jobDeclaration.PortOfLoading;

		ZString ITRCDetails.BookingConfirmationReference => jobDeclaration.AdditionalReferenceNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG)?.CE_EntryNum ?? ZString.Empty;

		CodeDescriptionPairList ITRCDetails.ContainerModeList => jobDeclaration.Lookups.CargoIdTypeList;
		ZString ITRCDetails.ContainerMode => jobDeclaration.JE_ContainerMode;

		CodeDescriptionPairList ITRCDetails.ShipmentTypeList => new CodeDescriptionPairList();
		ZString ITRCDetails.ShipmentType => ZString.Empty;

		ZString ITRCDetails.WaybillNumber => jobDeclaration.JE_MasterBill;

		OrgAddress ITRCDetails.ReceivingForwarder => jobDeclaration.Forwarder?.MainAddress;

		OrgAddress ITRCDetails.SendingForwarder => jobDeclaration.Forwarder?.MainAddress;

		OrgAddress ITRCDetails.Carrier => jobDeclaration.ShippingLine?.MainAddress;

		ZDateTime ITRCDetails.ETD => jobDeclaration.JE_DateAtOrigin;
		ZDateTime ITRCDetails.ETA => jobDeclaration.JE_DateAtFinalDestination;
		#endregion

	}
}
