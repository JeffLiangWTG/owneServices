using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AQISCollection<T> : NonPersistentBusinessObjectCollection<T> where T : NonPersistentBusinessObject
	{
		public AQISCollection(BusinessObjectFactory factory, AUAddInfo addInfo)
			: base(factory)
		{
			this.AddInfo = addInfo;
		}

		public void SplitAndAddAQISElements(ZString addInfoField)
		{
			if (this.Count > 0)
			{
				RemoveAll();
			}

			ZString[] spiltValues = addInfoField.Split(',');

			foreach (ZString currentValue in spiltValues)
			{
				if (!currentValue.IsEmpty)
				{
					ZString[] splitCurrentValue = currentValue.Split('/');

					if (splitCurrentValue.Length == 2)
					{
						Add(BusinessObjectToAddToCollection(splitCurrentValue[0], splitCurrentValue[1]));
					}
				}
			}
		}

		protected abstract BusinessObject BusinessObjectToAddToCollection(ZString value1, ZString value2);
		public abstract void ReBuildAndSaveAQISElements();

		protected readonly AUAddInfo AddInfo;

		public void SortByUniqueCode()
		{
			Sort(new AQISUniqueCodeCaseInsensitiveComparer());
		}
	}
}
