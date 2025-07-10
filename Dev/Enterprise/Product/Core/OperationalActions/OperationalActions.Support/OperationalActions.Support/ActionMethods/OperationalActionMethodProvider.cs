using System;

namespace Enterprise.Services.OperationalActions.Support
{
	public abstract class OperationalActionMethodProvider
	{
		#region New

		public static OperationalActionMethodProvider New(ActionMethodProviderID id)
		{
			if (id == null)
			{
				throw new ArgumentNullException(nameof(id));
			}

			var type = Type.GetType(id.ProviderFullName) ?? throw new ArgumentException("provider type not found", nameof(id));

			return (OperationalActionMethodProvider)Activator.CreateInstance(type);
		}

		#endregion

		protected OperationalActionMethodProvider()
		{
		}

		public abstract OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter);
	}
}
