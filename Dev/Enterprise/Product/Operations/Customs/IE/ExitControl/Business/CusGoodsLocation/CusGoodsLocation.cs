using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusGoodsLocation : Customs.Business.CusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("{CCCFEAF0-A17F-4B27-9D9D-B0F06EF48218}", Caption = "Type of Location")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.TypeOfLocationList))]
		[ReadOnlyMember(nameof(CGL_Type_ReadOnly))]
		public override ZString CGL_Type { get => base.CGL_Type; set => base.CGL_Type = value; }
		protected bool CGL_Type_ReadOnly => !IsPresentation;

		[ResourceStringData("{EF32FAA5-5896-467D-A392-A9D71F1FA55C}", Caption = "UNLOCO")]
		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.UNLOCOs))]
		[ReadOnlyMember(nameof(CGL_AdditionalIdentifier_ReadOnly))]
		[MaxLength(RefUNLOCO.Schema.RL_CodeMaxLength)]
		public override ZString CGL_AdditionalIdentifier { get => base.CGL_AdditionalIdentifier; set => base.CGL_AdditionalIdentifier = value; }
		protected bool CGL_AdditionalIdentifier_ReadOnly => !IsPresentation;

		public bool IsPresentation => ExitReportParent?.IsPresentation ?? false;

		public CusExitReport ExitReportParent => (CusExitReport)Parent;
		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		protected override TypeLoaderCollection GetParentLoaders() => new TypeLoaderCollection(typeof(CusExitReport));
	}
}
