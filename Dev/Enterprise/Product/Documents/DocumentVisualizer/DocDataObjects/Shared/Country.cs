using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	public sealed partial class Country : DocDataObject, ICountry
	{
		public Country(BusinessObjectFactory factory, IRefCountryCollection countries)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			Countries = Argument.NotNull(countries, nameof(countries));
		}

		#region Code

		[MaxLength(NotificationTypes.MessageError, 2)]
		public ZString Code
		{
			get => code;
			set
			{
				if (SetNonPersistentPropertyValue(CodeInfo, ref code, value))
				{
					if (!CodeInfo.IsOnValueChangedSuspended)
					{
						var country = Factory.LoadTop1<IRefCountry>(new ZQuery(RefCountrySchema.RN_Code, code));
						Name = country?.RN_Desc ?? ZString.Empty;
					}
				}
			}
		}

		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		#endregion

		#region Name

		[MaxLength(NotificationTypes.MessageError, 35)]
		public ZString Name
		{
			get => name;
			set
			{
				if (SetNonPersistentPropertyValue(NameInfo, ref name, value))
				{
					Validate(NameInfo);
				}
			}
		}

		ZString name;

		public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

		#endregion

		public IRefCountryCollection Countries { get; }
	}
}
