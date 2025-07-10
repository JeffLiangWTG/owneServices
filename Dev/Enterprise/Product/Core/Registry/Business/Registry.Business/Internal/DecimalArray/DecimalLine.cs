using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business.Internal
{
	public class DecimalLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		internal DecimalLine(DecimalLineCollection parentCollection)
		{
			this.parentCollection = parentCollection;
		}

		internal DecimalLineCollection ParentCollection
		{
			get { return parentCollection; }
		}

		readonly DecimalLineCollection parentCollection;

		#region Decimal Places

		public ZInt DecimalPlaces
		{
			get { return ParentCollection.DataType.DecimalPlaces; }
		}

		public ZPropertyInfo DecimalPlacesInfo
		{
			get { return GetZPropertyInfo(nameof(DecimalPlaces)); }
		}

		#endregion

		#region Number

		public ZDecimal Number
		{
			get { return number; }
			set
			{
				SetNonPersistentPropertyValue<ZDecimal>(NumberInfo, ref number, value);
				if (!IsValidationSuspended)
				{
					ValidateNumber();
				}
			}
		}

		public ZPropertyInfo NumberInfo
		{
			get { return GetZPropertyInfo(nameof(Number)); }
		}

		public void ValidateNumber()
		{
			NumberInfo.ClearAllNotifications();
			if (ParentCollection.DataType.LowerBound.HasValue)
			{
				CompareValidation.CheckGreaterThanOrEqualTo(NumberInfo, ParentCollection.DataType.LowerBound.Value);
			}
			if (ParentCollection.DataType.UpperBound.HasValue)
			{
				CompareValidation.CheckLessThanOrEqualTo(NumberInfo, ParentCollection.DataType.UpperBound.Value);
			}
		}

		ZDecimal number;

		#endregion
	}
}
