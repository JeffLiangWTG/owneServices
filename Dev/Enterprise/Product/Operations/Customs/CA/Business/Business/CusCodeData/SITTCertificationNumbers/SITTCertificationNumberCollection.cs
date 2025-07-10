using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class SITTCertificationNumberCollection : CusCodeDataCollection<SITTCertificationNumber>
	{
		public SITTCertificationNumberCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.SITTNumber)
		{
		}

		public new SITTCertificationNumber AddNew(ZString registrationNumber)
		{
			return AddNew(CusCodeDataTypeList.Codes.SITTNumber, registrationNumber);
		}

		public void Add(ZString regNumsAsAString)
		{
			if (!regNumsAsAString.IsEmpty)
			{
				ZString[] sITTCertNums = regNumsAsAString.Split(';', ',');
				foreach (string sITTCertNum in sITTCertNums)
				{
					if (!ContainsNumber(sITTCertNum))
					{
						AddNew(sITTCertNum);
					}
				}
			}
		}

		public bool ContainsNumber(string number)
		{
			return this.Cast<SITTCertificationNumber>().Any(element => element.CY_Data == number);
		}
	}
}
