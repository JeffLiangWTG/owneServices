using System;
using CargoWise.Async;

namespace CargoWise.EntityFramework.Business
{
	public abstract class BusinessObjectFilter : IBusinessObjectFilter
	{
		public void IsMatching(BusinessObject bizObj, IThreadSentry callbackSentry, BusinessObjectFilterIsMatchingCallback callback)
		{
			bizObj.Factory.ThreadSentry.Post((state) =>
				{
					var callbackData = new ThreadSentryCallbackData<BusinessObject, bool>(bizObj);
					try
					{
						callbackData.Data2 = IsMatching(bizObj);
					}
					catch (NotSupportedException)
					{
						//Ignore NotSupportedException
					}
					callbackSentry.Post((callbackState) =>
						{
							callback(callbackData);
						},
						callbackData, FormattableString.Invariant($"{nameof(BusinessObjectFilter)}.{nameof(IsMatching)}.{nameof(callbackSentry)}"));
				},
				null, FormattableString.Invariant($"{nameof(BusinessObjectFilter)}.{nameof(IsMatching)}"));
		}

		bool IBusinessObjectFilter.IsMatching(BusinessObject bizo) => IsMatching(bizo);

		protected abstract bool IsMatching(BusinessObject bizObj);
	}
}
