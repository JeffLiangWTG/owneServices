using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region ZAddressDropEditColumnStyle

	public class ZAddressDropEditColumnStyle : ZGuidDropEditColumnStyle
	{
		public ZAddressDropEditColumnStyle(ZAddressDropEditColumnStyleInfo columnInfo)
			: base(columnInfo, () => new ZAddressGridGuidDropEdit())
		{
		}
	}

	#endregion

	#region ZAddressDropEditColumnStyleInfo

	public class ZAddressDropEditColumnStyleInfo : ZGuidDropEditColumnStyleInfo, IZColumnStyleInfo, IZAddressDropEditColumnStyleInfo
	{
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Type ColumnStyleType
		{
			get { return typeof(ZAddressDropEditColumnStyle); }
		}
	}

	#endregion

	#region ZAddressGridGuidDropEdit

	class ZAddressGridGuidDropEdit : ZGridGuidDropEdit, IZAddressDropEdit
	{
		public ZAddressDropButton AddressDropButton => DropButton as ZAddressDropButton;

		#region NewDropButton

		protected override ZDropButton NewDropButton()
		{
			return new ZAddressDropButton();
		}

		#endregion

		#region BizObj

		BusinessObject BizObj
		{
			get { return (BusinessObject)CurrentItem; }
		}

		#endregion

		#region IZAddressDropEdit Members

		IZAddress IZAddressDropEdit.Addy
		{
			get
			{
				var result = BizObj as IZAddress;

				if (result == null)
				{
					if (DataPropertyName.EndsWith(JobDocAddressSchema.Constants.E2_OA_Address, StringComparison.InvariantCulture))
					{
						var addressPropertyName = DataPropertyName.Replace("+" + JobDocAddressSchema.Constants.E2_OA_Address, "");
						result = string.IsNullOrEmpty(addressPropertyName) || BizObj == null || BizObj.IsDeleted ? null : (IZAddress)BizObj[addressPropertyName];
					}
				}

				if (result == null)
				{
					var addressPropertyName = DataPropertyName;
					if (addressPropertyName != null && !addressPropertyName.EndsWith(ZAddress.Schema.BindingSuffix, StringComparison.InvariantCulture))
					{
						addressPropertyName += ZAddress.Schema.BindingSuffix;
					}
					try
					{
						result = addressPropertyName == null || BizObj == null || BizObj.IsDeleted ? null : (IZAddress)BizObj[addressPropertyName];
					}
					catch (Exception ex)
					{
						if (ex.IsCriticalException())
						{
							throw;
						}
					}
				}

				return result;
			}
		}

		bool IZAddressDropEdit.FilterAddressedByDefaultType
		{
			get { return filterAddressedByDefaultType; }
			set
			{
				filterAddressedByDefaultType = value;
				UpdateDropDown();
			}
		}
		bool filterAddressedByDefaultType = true;

		#endregion

		#region Overrides

		protected override IList GetList(object dataSource, string bindToList, string dataMemberForErrorReporting)
		{
			var addy = ((IZAddressDropEdit)this).Addy;
			IList list = (addy != null && !addy.IsDeleted ? addy.AddressList : null)
				?? base.GetList(dataSource, bindToList, dataMemberForErrorReporting);

			return list;
		}

		protected override IList GetFilteredListForDropDown()
		{
			var defaultAddressType = ((IZAddressDropEdit)this).Addy != null ? ((IZAddressDropEdit)this).Addy.DefaultAddressType : AddressType.NoDefault;
			if (((IZAddressDropEdit)this).FilterAddressedByDefaultType && defaultAddressType != AddressType.NoDefault)
			{
				if (base.List != null)
				{
					var filteredList = new CodeDescriptionPairList();
					foreach (var addressItem in base.List.OfType<ZAddressItem>())
					{
						if (addressItem.GetCapability(defaultAddressType.ToString()) != null ||
							((defaultAddressType == AddressType.DLV || defaultAddressType == AddressType.PIC) && addressItem.GetCapability(OrgConstants.AddressType.PickupAndDelivery) != null))
						{
							filteredList.Add(addressItem);
						}
					}

					return filteredList;
				}
			}

			return List;
		}

		#endregion
	}

	#endregion
}
