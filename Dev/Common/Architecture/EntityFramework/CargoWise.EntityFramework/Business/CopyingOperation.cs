using System;
using CargoWise.Common.Testing;

namespace CargoWise.EntityFramework
{
	/// <summary>
	/// Usage:
	///		using (new CopyingOperation(MyBusinessObject))
	///		{
	///			// set values with IsCopying = true and validation suspended
	///		}
	/// </summary>
	public sealed class CopyingOperation : IDisposable
	{
		public CopyingOperation(BusinessObject bO)
		{
			businessObject = bO;
			internals = bO;
			if (internals.IsCopying)
			{
				Dispose();
				throw new NotSupportedException("You can't use a CopyingOperation when IsCopying on the BizO is already true, as CopyingOperaton resets the IsCopying flag when it is disposed.");
			}
			internals.IsCopying = true;
			validationSuspender = bO.GetValidationSuspender();
			listChangedSuspender = bO.SuspendListChanged();
			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		readonly IBusinessObjectInternals internals;
		readonly BusinessObject businessObject;
		readonly IDisposable validationSuspender;
		readonly IDisposable listChangedSuspender;

		public void Dispose()
		{
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
			if (internals != null)
			{
				internals.IsCopying = false;
			}
			if (listChangedSuspender != null)
			{
				listChangedSuspender.Dispose();
			}
			if (validationSuspender != null)
			{
				validationSuspender.Dispose();
			}
			if (businessObject != null)
			{
				businessObject.OnElementChanged();
			}
		}
	}
}
