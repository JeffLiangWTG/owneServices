using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CFIARegistrationNumberCollection : CusCodeDataCollection<CFIARegistrationNumber>
	{
		public CFIARegistrationNumberCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.CFIANumber)
		{
		}

		public void Add(ZString cFIARegNumsAsAString)
		{
			if (!cFIARegNumsAsAString.IsEmpty)
			{
				ZString[] cfiaRegNums = cFIARegNumsAsAString.Split(';', ',');
				foreach (string cFIARegNum in cfiaRegNums)
				{
					string[] cFIARegNumElements = cFIARegNum.Split('/');
					if (cFIARegNumElements.Length == 2)
					{
						var existingRegNum = GetFirstElementHaving(cFIARegNumElements[0]);
						if (existingRegNum == null)
						{
							AddNew(cFIARegNumElements[0], cFIARegNumElements[1]);
						}
						else
						{
							existingRegNum.CY_Data = cFIARegNumElements[1];
						}
					}
				}
			}
		}

		public bool ContainsRegNum(ZString code, ZString regNo)
		{
			return this.Cast<CFIARegistrationNumber>().Any(x => x.CY_Code == code && x.CY_Data == regNo);
		}
	}
}
