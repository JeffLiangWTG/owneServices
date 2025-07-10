using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitControlHeader : AutoCusExitControlHeader, Integration.Customs.EU.ICusExitControlHeader
	{
		public CusExitControlHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static readonly CusExitControlHeaderTypeDecider TypeDecider = new CusExitControlHeaderTypeDecider();

		public BusinessObject CEH_Parent
		{
			get
			{
				if (parent == null || parent.IsDeleted || parent.PK != CEH_ParentID)
				{
					parent = !CEH_ParentID.IsEmpty && !CEH_ParentTableCode.IsEmpty
						? Factory.Load(CEH_ParentTableCode, CEH_ParentID)
						: null;
				}
				return parent;
			}
			set
			{
				parent = value;
				if (parent != null && (CEH_ParentID != parent.PK || CEH_ParentTableCode != parent.TablePrefix))
				{
					CEH_ParentID = parent.PK;
					CEH_ParentTableCode = parent.TablePrefix;
				}
			}
		}
		BusinessObject parent;

		[List(nameof(Lookups) + "." + nameof(CusExitControlHeaderLookups.OrganizationsFindBoxList))]
		public override ZGuid CEH_OA_Agent
		{
			get => base.CEH_OA_Agent;
			set
			{
				base.CEH_OA_Agent = value;
				CEH_OA_AgentInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusExitControlHeaderLookups.OrganizationsFindBoxList))]
		public override ZGuid CEH_OA_Carrier
		{
			get => base.CEH_OA_Carrier;
			set
			{
				base.CEH_OA_Carrier = value;
				CEH_OA_CarrierInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusExitControlHeaderLookups.CustomsOffices))]
		[ResourceStringData("Enterprise.Customs.EU.Business.CusExitControlHeader|CEH_CustomsOffice", Caption = "Exit Customs Office")]
		public override ZString CEH_CustomsOffice { get => base.CEH_CustomsOffice; set => base.CEH_CustomsOffice = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.CusExitControlHeader|CEH_ArrivalNotificationDate", Caption = "Arrival Notification Date")]
		public override ZDateTime CEH_ArrivalNotificationDate { get => base.CEH_ArrivalNotificationDate; set => base.CEH_ArrivalNotificationDate = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.CusExitControlHeader|CEH_ArrivalNotificationPlace", Caption = "Arrival Notification Place")]
		public override ZString CEH_ArrivalNotificationPlace { get => base.CEH_ArrivalNotificationPlace; set => base.CEH_ArrivalNotificationPlace = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.CusExitControlHeader|CEH_ExitDate", Caption = "Exit Date")]
		public override ZDateTime CEH_ExitDate { get => base.CEH_ExitDate; set => base.CEH_ExitDate = value; }

		[ResourceStringData("Enterprise.Customs.EU.Business.CusExitControlHeader|CEH_TransportID", Caption = "Transport ID")]
		public override ZString CEH_TransportID { get => base.CEH_TransportID; set => base.CEH_TransportID = value; }

		[ChildEditable(true)]
		public CusExitDetailCollection CusExitDetails
		{
			get
			{
				if (cusExitDetails == null)
				{
					cusExitDetails = GetNewCusExitDetailsCollectionCore();
					cusExitDetails.Load();
					RegisterEditableChildObject(cusExitDetails);
				}
				return cusExitDetails;
			}
		}

		CusExitDetailCollection cusExitDetails;

		public override void Delete()
		{
			CusExitDetails.RemoveAndDeleteAll();
			base.Delete();
		}

		protected virtual CusExitDetailCollection GetNewCusExitDetailsCollectionCore() => new CusExitDetailCollection(this);

		public JobDeclaration Declaration => CEH_ParentTableCode == JobDeclarationSchema.Constants.Prefix ? CEH_Parent as JobDeclaration : null;

		public ZString DataGrouping => Declaration?.GetDefaultDataGroupingCode() ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public ZString CountryCode => Declaration?.CountryCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			CEH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			CEH_ParentID = Guid.NewGuid();
			CEH_ReferenceNumber = CEH_ParentID.ToString();
			CEH_ArrivalNotificationDate = ZDate.Today;
		}
#endif
	}
}
