using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerTextField : RunnerTextField<OperationalActionTextFieldSupporter>
	{
		public RunnerTextField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionTextFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }
	}

	public abstract class RunnerTextField<TSupporter> : RunnerField<TSupporter, ZString>
		where TSupporter : OperationalActionTextFieldSupporter
	{
		protected RunnerTextField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, TSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		#region Property

		protected override void SetPropertyCore(ZString value)
		{
			CheckMaximumLength(PropertyInfo, value);
			base.SetPropertyCore(value);
		}

		protected override ZPropertyInfo GetPropertyInfoCore()
		{
			return FieldSupporter == null ? null : GetZPropertyInfo(Schema.Property, Caption);
		}

		public int Property_MaxLength { get { return FieldSupporter.MaxLength; } }

		#endregion
	}
}
