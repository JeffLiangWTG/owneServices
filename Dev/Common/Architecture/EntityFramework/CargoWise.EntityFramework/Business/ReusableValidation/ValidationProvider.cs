namespace CargoWise.EntityFramework
{
	public abstract class ValidationProvider
	{
		/// <summary>
		/// Use to encapsulate complex validation that would only clutter your business object.
		/// See MandatoryValidation for an example ValidationProvider.
		/// </summary>
		protected ValidationProvider()
		{
		}

		/// <summary>
		/// Use to encapsulate complex validation that would only clutter your business object.
		/// See MandatoryValidation for an example ValidationProvider.
		/// </summary>
		/// <param name="factoryProvider">An object that provides factory capabilities.</param>
		protected ValidationProvider(IFactoryProvider factoryProvider)
		{
			this.FactoryProvider = factoryProvider;
		}

		#region Implementation

		protected BusinessObjectFactory Factory
		{
			get { return FactoryProvider != null ? FactoryProvider.Factory : null; }
		}

		protected readonly IFactoryProvider FactoryProvider;

		#endregion
	}
}
