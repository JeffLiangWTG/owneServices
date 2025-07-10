using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSRegistrationNumberCollection : CusCodeDataCollection<AIRSRegistrationNumber>
	{
		public AIRSRegistrationNumberCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.AIRSNumber)
		{
		}

		public void CopyPersistentValuesFrom(AIRSRegistrationNumberCollection source)
		{
			RemoveAndDeleteAll();
			foreach (BusinessObject sourceElement in source)
			{
				BusinessObject targetElement = AddNew();
				targetElement.CopyPersistentValuesFrom(sourceElement);
			}
		}

		public AIRSRegistrationNumber AddNewIfNotExist(ZString code)
			=> this.Cast<AIRSRegistrationNumber>().FirstOrDefault(x => x.CY_Code == code)
				?? this.AddNew(code);
	}
}
