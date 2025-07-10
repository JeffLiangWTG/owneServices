using System;

namespace Enterprise.ZArchitecture.Environment
{
	public class PackLineRegistry
	{
		public PackLineRegistry(RawDataRegistry rawRegistry)
		{
			RawRegistry = rawRegistry;
		}

		public bool IsPackLineCustomAttribute1HintOverridden => PackLineCustomAttribute1Hint != (string)RawRegistry.PackLineCustomAttribute1Hint.DefaultValue;

		public bool IsPackLineCustomAttribute2HintOverridden => PackLineCustomAttribute2Hint != (string)RawRegistry.PackLineCustomAttribute2Hint.DefaultValue;

		public bool IsPackLineCustomAttribute3HintOverridden => PackLineCustomAttribute3Hint != (string)RawRegistry.PackLineCustomAttribute3Hint.DefaultValue;

		public bool IsPackLineCustomAttribute4HintOverridden => PackLineCustomAttribute4Hint != (string)RawRegistry.PackLineCustomAttribute4Hint.DefaultValue;

		public bool IsPackLineCustomDecimal1HintOverridden => PackLineCustomDecimal1Hint != (string)RawRegistry.PackLineCustomDecimal1Hint.DefaultValue;

		public bool IsPackLineCustomDecimal2HintOverridden => PackLineCustomDecimal2Hint != (string)RawRegistry.PackLineCustomDecimal2Hint.DefaultValue;

		public bool IsPackLineCustomFlag1HintOverridden => PackLineCustomFlag1Hint != (string)RawRegistry.PackLineCustomFlag1Hint.DefaultValue;

		public bool IsPackLineCustomFlag2HintOverridden => PackLineCustomFlag2Hint != (string)RawRegistry.PackLineCustomFlag2Hint.DefaultValue;

		public bool IsPackLineCustomDate1HintOverridden => PackLineCustomDate1Hint != (string)RawRegistry.PackLineCustomDate1Hint.DefaultValue;

		public bool IsPackLineCustomDate2HintOverridden => PackLineCustomDate2Hint != (string)RawRegistry.PackLineCustomDate2Hint.DefaultValue;

		public string PackLineCustomAttribute1Caption
		{
			get { return (string)RawRegistry.PackLineCustomAttribute1Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute1Hint
		{
			get { return (string)RawRegistry.PackLineCustomAttribute1Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute2Caption
		{
			get { return (string)RawRegistry.PackLineCustomAttribute2Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute2Hint
		{
			get { return (string)RawRegistry.PackLineCustomAttribute2Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute2Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute3Caption
		{
			get { return (string)RawRegistry.PackLineCustomAttribute3Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute3Hint
		{
			get { return (string)RawRegistry.PackLineCustomAttribute3Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute3Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute4Caption
		{
			get { return (string)RawRegistry.PackLineCustomAttribute4Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute4Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomAttribute4Hint
		{
			get { return (string)RawRegistry.PackLineCustomAttribute4Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomAttribute4Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDecimal1Caption
		{
			get { return (string)RawRegistry.PackLineCustomDecimal1Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDecimal1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDecimal1Hint
		{
			get { return (string)RawRegistry.PackLineCustomDecimal1Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDecimal1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDecimal2Caption
		{
			get { return (string)RawRegistry.PackLineCustomDecimal2Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDecimal2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDecimal2Hint
		{
			get { return (string)RawRegistry.PackLineCustomDecimal2Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDecimal2Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDate1Caption
		{
			get { return (string)RawRegistry.PackLineCustomDate1Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDate1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDate1Hint
		{
			get { return (string)RawRegistry.PackLineCustomDate1Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDate1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomFlag1Caption
		{
			get { return (string)RawRegistry.PackLineCustomFlag1Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomFlag1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomFlag1Hint
		{
			get { return (string)RawRegistry.PackLineCustomFlag1Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomFlag1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDate2Caption
		{
			get { return (string)RawRegistry.PackLineCustomDate2Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDate2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomDate2Hint
		{
			get { return (string)RawRegistry.PackLineCustomDate2Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomDate2Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomFlag2Caption
		{
			get { return (string)RawRegistry.PackLineCustomFlag2Caption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomFlag2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomFlag2Hint
		{
			get { return (string)RawRegistry.PackLineCustomFlag2Hint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomFlag2Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string PackLineCustomHeadingHint
		{
			get { return (string)RawRegistry.PackLineCustomHeadingHint.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public string PackLineCustomHeadingCaption
		{
			get { return (string)RawRegistry.PackLineCustomHeadingCaption.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
#if DEBUG
			set { RawRegistry.PackLineCustomHeadingCaption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		readonly RawDataRegistry RawRegistry;
	}
}
