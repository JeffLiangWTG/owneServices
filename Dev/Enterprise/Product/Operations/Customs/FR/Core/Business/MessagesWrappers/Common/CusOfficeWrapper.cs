using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class CusOfficeWrapper : ICusOffice
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CusOfficeWrapper(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "CustEntryHeader cannot be null");
			this.declaration = Argument.NotNull(this.entryHeader.Declaration, "JobDeclaration cannot be null");
		}

		#region Members
		public ZString OfficeOfDeclaration => declaration.OfficeOfDeclaration;

		public ZString OfficeOfLodgement => declaration?.JE_CustomsOffice ?? ZString.Empty;

		public ZString VisitingOffice => new ZString(null);

		#region Export
		public ZString ExitOffice => GetDeclarationOffice(EuOfficeCodesTypes.Codes.OfficeOfExit);

		public ZString ECSExitType => GetExitTypeNumberCode(declaration?.JE_ExportExitType ?? ZString.Empty);

		public ZString ECSMotivation => declaration?.JE_ExportExitTypeReason ?? ZString.Empty;
		#endregion

		#endregion

		#region Methods
		ZString GetExitTypeNumberCode(ZString ecsCode)
		{
			switch (ecsCode)
			{
				case ExportExitTypeList.Codes.ECS:
					return "01";
				case ExportExitTypeList.Codes.STC:
					return "02";
				case ExportExitTypeList.Codes.EMC:
					return "03";
				case ExportExitTypeList.Codes.TRA:
					return "04";
				case ExportExitTypeList.Codes.OTH:
					return "99";
				default:
					return "";
			}
		}

		ZString GetDeclarationOffice(ZString code)
		{
			EuOfficeCodeCollection customsOffices = declaration.CustomsOffices;

			var customOffice = customsOffices.Cast<EuOfficeCode>().ToList().FirstOrDefault(a => a.CY_Code == code
																					&& a.CY_Type == EU.Business.CusCodeDataTypeList.Codes.OfficeCode
																					&& a.CY_Data != ZString.Empty);

			return customOffice?.CY_Data ?? ZString.Empty;
		}

		#endregion

		protected readonly CusEntryHeader entryHeader;
		protected readonly JobDeclaration declaration;
	}
}
