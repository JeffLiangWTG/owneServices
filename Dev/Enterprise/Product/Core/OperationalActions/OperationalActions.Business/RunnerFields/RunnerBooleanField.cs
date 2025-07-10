using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerBooleanField : RunnerField<OperationalActionBooleanFieldSupporter, ZString>
	{
		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public new class Schema : RunnerField<OperationalActionBooleanFieldSupporter, ZString>.Schema
		{
			public const string Property_List = "Property_List";
		}

		#endregion

		public RunnerBooleanField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionBooleanFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		public BooleanChangeType Property_List
		{
			get { return property_List ?? (property_List = new BooleanChangeType()); }
		}

		#region Implementation

		[MaxLength(3)]
		public override ZString Property
		{
			get { return base.Property; }
			set { base.Property = value; }
		}

		protected override IZType GetValueCore(Type expectedType)
		{
			return (ZBool)(Property == BooleanChangeType.Codes.Set);
		}

		protected override void SetPropertyCore(ZString value)
		{
			CheckMaximumLength(PropertyInfo, value);
			base.SetPropertyCore(value);
		}

		protected override ZPropertyInfo GetPropertyInfoCore()
		{
			return GetZPropertyInfo(Schema.Property, Caption);
		}

		public int Property_MaxLength { get { return 3; } }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();

			ListValidation.ErrorIfInvalidCode(PropertyInfo, Property_List);
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		BooleanChangeType property_List;

		#endregion
	}
}
