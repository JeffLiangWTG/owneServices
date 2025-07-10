using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.DataMapping
{
	[CodeProperty(CustomMapPairListWrapper.Schema.Name), DescriptionProperty(CustomMapPairListWrapper.Schema.Name)]
	public class CustomMapPairListWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public abstract class Schema
		{
			public const string TableName = "CustomMapPairListWrapper";
			public const string Name = "Name";
		}

		#region Name

		[CargoWise.ComponentModel.MaxLength(50)]
		public ZString Name
		{
			get { return name; }
			set { SetNonPersistentPropertyValue(NameInfo, ref name, value); }
		}

		ZString name;

		public ZPropertyInfo NameInfo
		{
			get { return GetZPropertyInfo(nameof(Name)); }
		}

		#endregion

		#region List

		public CustomMapPairList List
		{
			get
			{
				if (list == null)
				{
					list = new CustomMapPairList();
				}

				return list;
			}
		}

		CustomMapPairList list;

		#endregion
	}

	public class CustomMapPairListWrapperCollection : NonPersistentBusinessObjectCollection<CustomMapPairListWrapper>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomMapPairListWrapper();
		}

		public CustomMapPairListWrapper Find(string name)
		{
			foreach (CustomMapPairListWrapper w in this)
			{
				if (w.Name == name)
				{
					return w;
				}
			}

			return null;
		}
	}
}
