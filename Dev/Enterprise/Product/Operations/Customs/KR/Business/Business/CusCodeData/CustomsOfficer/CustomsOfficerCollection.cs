using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CustomsOfficerCollection : CusCodeDataCollection<CustomsOfficer>
	{
		public CustomsOfficerCollection(CusEntryHeader entryHeader)
			: base(entryHeader, CusCodeDataTypeList.Codes.CustomsOfficer)
		{
		}

		public void CreateOrUpdate(ZString type, ZString id, ZString name, ZDateTime modifyDate)
		{
			var officerInfo = string.Join("-", id, name);
			if (id.IsEmpty || name.IsEmpty)
			{
				officerInfo = id + name;
			}
			var existingOfficer = this.OfType<CustomsOfficer>().FirstOrDefault(x => x.CY_Code == type && x.CY_Date == modifyDate);
			if (existingOfficer == null)
			{
				var newOfficer = AddNew();
				newOfficer.CY_Code = type;
				newOfficer.CY_Data = officerInfo;
				newOfficer.CY_Date = modifyDate;
			}
			else if (existingOfficer.CY_Data != officerInfo)
			{
				existingOfficer.CY_Data = officerInfo;
			}
		}

		public CustomsOfficer this[string index]
		{
			get
			{
				var regex = new Regex(@"^\s*""(\s*(?<customsOfficerCode>[A-Za-z0-9]{3,4})"")\s*$");
				var parameters = regex.Match(index);
				if (parameters.Success)
				{
					string customsOfficerCode = parameters.Groups["customsOfficerCode"].Value;
					return this.Cast<CustomsOfficer>().FirstOrDefault(x => x.CY_Code == customsOfficerCode);
				}
				return null;
			}
		}

		public ZString GetCustomsOfficer(string type) => this.Cast<CustomsOfficer>().FirstOrDefault(x => x.CY_Code == type)?.CY_Data ?? ZString.Empty;
	}
}
