using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgMiscServ : OrgMiscServ
	{
		public UPEOrgMiscServ(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessOverrides

		public UPEOrgHeader Parent => ((UPEOrgHeader)Header);

		public override ZPropertyInfo OM_CustomFlag4Info
		{
			get
			{
				((IZPropertyInfoObsolete)base.OM_CustomFlag4Info).ReadOnly = !GlbStaff.CurrentUser.GS_IsController;
				return base.OM_CustomFlag4Info;
			}
		}

		#endregion

		public ZBool LOAReceivedAuthorisingUPStoClearGoods
		{
			get
			{
				ZBool result = false;
				if (base.OM_CustomAttrib3 == "Y")
				{
					result = true;
				}
				return result;
			}
			set
			{
				base.OM_CustomAttrib3 = value.ToString();
				LOAReceivedAuthorisingUPStoClearGoodsInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LOAReceivedAuthorisingUPStoClearGoodsInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(LOAReceivedAuthorisingUPStoClearGoodsName); }
		}

		public ZBool IsPreReleaseFeeApplicable { get => Parent.IsPreReleaseContactFeeApplicable; set => Parent.IsPreReleaseContactFeeApplicable = value; }

		public const string LOAReceivedAuthorisingUPStoClearGoodsName = "LOAReceivedAuthorisingUPStoClearGoods";
	}
}
