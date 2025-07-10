using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class CusCAeMHItem : AutoCusCAeMHItem,
		IUNDGDataItemProvider,
		IHouseBillLine,
		Customs.Business.ISynchroniserReadOnlyMembersProvider
	{
		public CusCAeMHItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		public new class Schema : AutoCusCAeMHItem.Schema
		{
			public const string FirstUNDG = "FirstUNDG";
		}

		#region Overriden

		[List(nameof(Lookups) + "." + nameof(CusCAeMHItemLookups.QuantityUnits))]
		public override ZString BX_QuantityUQ
		{
			get { return base.BX_QuantityUQ; }
			set { base.BX_QuantityUQ = value; }
		}

		public ZString BX_QuantityUQDescription
		{
			get { return Lookups.QuantityUnits.GetDescriptionFromCode(this.BX_QuantityUQ); }
		}

		[RelatedBusinessObject("HouseBill")]
		public override ZGuid BX_BW_House
		{
			get { return base.BX_BW_House; }
			set { base.BX_BW_House = value; }
		}

		public CusCAeMHHouse HouseBill
		{
			get { return Factory.Load<CusCAeMHHouse>(this.BX_BW_House); }
		}

		#endregion

		#region New Properties

		#region FirstUNDG

		[List(nameof(UNDG) + "." + nameof(UNDGDataItem.Lookups) + "." + nameof(UNDGDataItemLookups.UNDGSubstances))]
		public ZGuid FirstUNDG
		{
			get { return UNDG?.DI_DG ?? ZGuid.Empty; }
			set
			{
				var hasChanges = FirstUNDG != value;
				if (hasChanges)
				{
					if (fUNDG == null)
					{
						fUNDG = UNDGs.AddNew();
						RegisterEditableChildObject(fUNDG);
					}
					fUNDG.DI_DG = value;
				}
				if (!IsValidationSuspended)
				{
					Validation.ValidateFirstUNDG();
				}
				FirstUNDGInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FirstUNDGInfo
		{
			get { return GetZPropertyInfo(Schema.FirstUNDG); }
		}

		#endregion

		public UNDGDataItem UNDG
		{
			get
			{
				if (fUNDG == null || fUNDG.IsDeleted)
				{
					fUNDG = UNDGs.FirstOrDefault();
				}

				return fUNDG;
			}
		}
		UNDGDataItem fUNDG;

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new UNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}
				return fUNDGs;
			}
		}
		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		#endregion

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region IHouseBillLine

		ZInt IHouseBillLine.LineNumber
		{
			get { return BX_LineNumber; }
		}

		ZDecimal IHouseBillLine.Packs
		{
			get { return BX_Quantity; }
		}
		ZString IHouseBillLine.PacksUOM
		{
			get { return BX_QuantityUQ; }
		}
		IEnumerable<ZString> IHouseBillLine.Marks
		{
			get { return new ZString[] { BX_Marks }; }
		}

		ZString IHouseBillLine.GoodsDescription
		{
			get { return BX_Description; }
		}

		ZString IHouseBillLine.HSCode
		{
			get { return BX_HSCode; }
		}

		IEnumerable<ZString> IHouseBillLine.DGCodes
		{
			get
			{
				if (BX_IsDangerousInBulk && HouseBill != null && HouseBill.MasterBill != null && HouseBill.MasterBill.IsSea)
				{
					return new ZString[] { "MHB" };
				}
				else
				{
					return (from UNDGDataItem undg in UNDGs select undg.UNDGSubstance != null ? undg.UNDGSubstance.DG_UNNO : ZString.Empty);
				}
			}
		}

		#endregion

		#region clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(CusCAeMHItemSchema.Constants.BX_BW_House);
			return result;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var templateCopy = (CusCAeMHItem)base.CloneInternal(args);
			foreach (var dg in UNDGs.ToArray())
			{
				templateCopy.UNDGs.Add((UNDGDataItem)dg.Clone());
			}
			return templateCopy;
		}

		#endregion

	}
}
