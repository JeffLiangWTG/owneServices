
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class AQISSingleValueCollection : NonPersistentBusinessObjectCollection<AQISSingleValueBusinessObject>
	{
		public AQISSingleValueCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public void SplitAndAddAQISElements(string addInfoStringToSplit)
		{
			if (this.Count > 0)
			{
				RemoveAll();
			}

			string[] spiltValues = addInfoStringToSplit.Split(',');

			foreach (ZString value in spiltValues)
			{
				if (!value.IsEmpty)
				{
					AQISSingleValueBusinessObject newBizObj = this.AddNew();
					using (newBizObj.SuspendSettingHasChanges())
					{
						if (value.Length <= newBizObj.CodeInfo.MaxLength)
						{
							newBizObj.Code = value;
						}
						else
						{
							this.RemoveAndDelete(newBizObj);
						}
					}
				}
			}
		}

		public ZString ReBuildAQISElements()
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (AQISSingleValueBusinessObject bizObj in this)
			{
				result.Append(bizObj.Code + ",");
			}

			return new ZString(result.ToString()).TrimEndIncludingWhiteSpace(',');
		}

		public void SortByUniqueCode()
		{
			Sort(new AQISUniqueCodeCaseInsensitiveComparer());
		}
	}
}
