using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <summary>
	/// A simple wrapper to encapsulate a document field label with its respective value together to make doc building easier and more intuitive.  You can
	/// label an object you like, just make sure it has a ToString() override you require. 
	/// </summary>
	/// <remarks>
	/// This wrapper also supports storing a label with two values, a formatted string value and the native object.  For example you can store
	/// ("Discount Amount:", "$12.57 AUD", 12.57839m), simply by passing in a LabelValuePairWrapper as the Object value.
	/// </remarks>
	/// <example>LabelValuePairWrapper DiscountAmount = new LabelValuePairWrapper("Discount Amount:", new LabelValuePairWrapper("$12.57 AUD", 12.57839m, Factory), Factory);</example>
	[DefaultField("Value")]
	public class LabelValuePairWrapper : GenericWrapper
	{
		#region Constructors

		public LabelValuePairWrapper(BusinessObjectFactory factory) : base(null, factory)
		{
			this.primaryValue = ZString.Empty;
			this.secondaryValue = ZString.Empty;
		}

		public LabelValuePairWrapper(String label, Object value, BusinessObjectFactory factory) : base(null, factory)
		{
			this.primaryValue = label;
			this.secondaryValue = value;
		}

		#endregion

		public ZString Label
		{
			get { return primaryValue; }
		}

		public ZString Value
		{
			get { return (secondaryValue is LabelValuePairWrapper) ? ((LabelValuePairWrapper)secondaryValue).primaryValue : SecondaryValueFormated; }
		}

		ZString SecondaryValueFormated
			=> secondaryValue != null
				? GetSecondaryValueAsString
				: ZString.Empty;

		ZString GetSecondaryValueAsString
			=> secondaryValue is ZDateTime time
				? time.ToLongTimeString()
				: secondaryValue.ToString();

		public ZDateTime ValueAsDate
		{
			get
			{
				if (secondaryValue is ZDateTime)
				{
					return (ZDateTime)secondaryValue;
				}
				else if (secondaryValue is LabelValuePairWrapper && ((LabelValuePairWrapper)secondaryValue).secondaryValue is ZDateTime)
				{
					return (ZDateTime)((LabelValuePairWrapper)secondaryValue).secondaryValue;
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		public ZDecimal ValueAsDecimal
		{
			get { return ZDecimal.ParseSafe(secondaryValue.ToString(), 0); }
		}

		internal Object NativeValue
		{
			get { return (secondaryValue is LabelValuePairWrapper) ? ((LabelValuePairWrapper)secondaryValue).NativeValue : secondaryValue; }
		}

		public ZString LabelAndValue
		{
			get { return !IsEmpty ? ZString.Format(inLineTextMask, Label, Value) : ZString.Empty; }
		}
		const string inLineTextMask = "{0}: {1}";

		internal static LabelValuePairWrapper Empty
		{
			get { return empty ?? (empty = new LabelValuePairWrapper(new BusinessObjectFactory())); }
		}
		[ThreadStatic]
		static LabelValuePairWrapper empty;

		#region Implementation

		internal bool IsEmpty
		{
			get
			{
				if (NativeValue is IZType)
				{
					return ((IZType)NativeValue).IsEmpty;
				}
				else
				{
					return Value.IsEmpty;
				}
			}
		}

		readonly ZString primaryValue;
		readonly Object secondaryValue;

		#endregion
	}
}
