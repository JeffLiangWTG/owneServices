using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyNonPersistentBusinessObjectWithoutAttribute : NonPersistentBusinessObject
	{
		[MaxLength(10)]
		public ZString Code
		{
			get { return code; }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				code = value;
				CodeInfo.RefreshBinding();
			}
		}
		ZString code;
		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		[MaxLength(20)]
		public ZString Description
		{
			get { return description; }
			set
			{
				CheckMaximumLength(DescriptionInfo, value);
				description = value;
				DescriptionInfo.RefreshBinding();
			}
		}
		ZString description;
		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		public DummyNonPersistentBusinessObjectValidation Validation
		{
			get { return new DummyNonPersistentBusinessObjectValidation(this); }
		}
	}
}
