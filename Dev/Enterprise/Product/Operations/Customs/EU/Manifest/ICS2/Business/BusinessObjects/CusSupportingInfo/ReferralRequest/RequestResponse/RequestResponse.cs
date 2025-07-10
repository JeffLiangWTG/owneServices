using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RequestResponse : CusSupportingInfo
	{
		public RequestResponse(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new RequestResponseValidation Validation => (RequestResponseValidation)base.Validation;
		protected override CusSupportingInfoValidation GetNewValidation() => new RequestResponseValidation(this);

		public new RequestResponseLookups Lookups => (RequestResponseLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new RequestResponseLookups(this);

		#region Properties

		[List(nameof(Lookups) + "." + nameof(RequestResponseLookups.CodeList))]
		[ResourceStringData("EUICS2.RequestResponse.CSI_Code", Caption = "Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (oldValue != CSI_Code && !IsCopying)
				{
					codeDescription = null;
				}
			}
		}

		[ResourceStringData("EUICS2.RequestResponse.CodeDescription", Caption = "Code Description")]
		public ZString CodeDescription => codeDescription ?? (codeDescription = GetCodeDescription());
		string codeDescription;

		string GetCodeDescription()
		{
			if (!CSI_Code.IsEmpty)
			{
				return Lookups.CodeList.Cast<ZZRefCusCodeListCombined>().FirstOrDefault(code => code.ZZD_Code == CSI_Code)?.ZZD_Description ?? string.Empty;
			}

			return string.Empty;
		}

		[List(nameof(Lookups) + "." + nameof(RequestResponseLookups.SubTypeList))]
		[ResourceStringData("EUICS2.RequestResponse.CSI_SubType", Caption = "Type")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_SubType = value;
				if (oldValue != CSI_SubType && !IsCopying)
				{
					subTypeDescription = null;
				}
			}
		}

		[ResourceStringData("EUICS2.RequestResponse.SubTypeDescription", Caption = "Type Description")]
		public ZString SubTypeDescription => subTypeDescription ?? (subTypeDescription = GetSubTypeDescription());
		string subTypeDescription;

		string GetSubTypeDescription()
		{
			if (!CSI_SubType.IsEmpty)
			{
				return Lookups.SubTypeList.GetDescriptionFromCode(CSI_SubType);
			}

			return string.Empty;
		}

		[ResourceStringData("EUICS2.RequestResponse.CSI_Description", Caption = "Additional Information")]
		public override ZString CSI_Description { get => base.CSI_Description; set => base.CSI_Description = value; }

		#endregion
	}
}
