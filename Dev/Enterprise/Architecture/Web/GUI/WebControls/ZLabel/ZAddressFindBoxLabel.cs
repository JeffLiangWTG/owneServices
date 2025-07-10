using System.ComponentModel;
using System.Web.UI;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	/// <summary>
	/// Label to display ZAddresses
	/// </summary>
	[DefaultProperty("Text"), ToolboxData("<{0}:ZAddressFindBoxLabel runat=server></{0}:ZAddressFindBoxLabel>")]
	public class ZAddressFindBoxLabel : ZFindBoxLabelBase
	{
		protected override string GetCode(object list, IZType value)
		{
			ZAddressItem address = GetAddressFromList(list as ZAddressList, (ZGuid)value);
			return address.AddressDescription;
		}

		protected override string GetDescription(object list, IZType value)
		{
			ZAddressItem address = GetAddressFromList(list as ZAddressList, (ZGuid)value);
			return address.AddressDescription;
		}

		protected ZAddressItem GetAddressFromList(ZAddressList list, ZGuid value)
		{
			foreach (ZAddressItem address in list)
			{
				if (address.PK == value)
				{
					return address;
				}
			}
			return null;
		}
	}
}
