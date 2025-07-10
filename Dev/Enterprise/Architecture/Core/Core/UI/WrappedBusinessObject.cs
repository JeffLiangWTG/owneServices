using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core
{
	[Serializable]
	public class WrappedBusinessObject
	{
		public WrappedBusinessObject(BusinessObject baseBusinessObject)
		{
			this.baseBusinessObject = baseBusinessObject;
		}

		public BusinessObject BaseBusinessObject
		{
			get { return baseBusinessObject; }
		}

		[NonSerialized]
		readonly BusinessObject baseBusinessObject;
	}
}
