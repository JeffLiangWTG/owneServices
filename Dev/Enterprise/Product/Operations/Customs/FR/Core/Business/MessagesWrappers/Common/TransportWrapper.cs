using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using SharedBusinness = Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class TransportWrapper : ITransport
	{
		public TransportWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");
			this.declaration = Argument.NotNull(this.entryHeader.Declaration, "JobDeclaration cannot be null");
		}

		public ZString ModeOfTRansport => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportMode, true);

		public ZString ModeOfTRansportInland => declaration.TransportModeTranslator.TranslateToWCOCode(declaration.JE_TransportModeInland, true);

		public ZString ContainerMode => declaration.IsContainerizedAndHasContainer ? "1" : "0";

		public ZString NationalityOfTransport => declaration.JE_RN_NKTransportNationality;

		public ZString CusOffice => GetCusOffice();

		public ZString IATAAirportOfLoading => declaration.JE_IATALoadPort;

		public ZString AirRoadType => declaration.JE_AirRouteType;

		#region Export
		public ZString TransportID
		{
			get
			{
				if (declaration.JE_TransportMode == SharedBusinness.TransportTypeList.Codes.Air)
				{
					return declaration.JE_VoyageFlightNo;
				}
				else
				{
					return declaration.JE_VesselName;
				}
			}
		}

		public ZString TransportMethodPayment => entryHeader.RandomHeader?.ZG_TransportChargesMethodOfPayment ?? ZString.Empty;
		#endregion

		ZString GetCusOffice()
		{
			EuOfficeCodeCollection customsOffices = declaration.CustomsOffices;
			EuOfficeCode customOffice = (from EuOfficeCode euOfficeCode in customsOffices
													 where euOfficeCode.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent
													 where euOfficeCode.CY_Type == EU.Business.CusCodeDataTypeList.Codes.OfficeCode
													 select euOfficeCode).FirstOrDefault();
			return customOffice?.CY_Data ?? ZString.Empty;
		}

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
	}
}
