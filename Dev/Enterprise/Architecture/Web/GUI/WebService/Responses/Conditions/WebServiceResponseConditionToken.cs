using System.Web.Script.Serialization;

namespace Enterprise.ZArchitecture.Web.ServerServices
{
	#region SuppressResourceStringsCheckRegion

	public abstract class WebServiceResponseConditionToken : WebServiceResponseToken
	{
		#region Constructors

		public WebServiceResponseConditionToken()
			: base()
		{
		}

		public WebServiceResponseConditionToken(string condition, string controlID, object conditionValue)
			: base(controlID, conditionValue?.ToString() ?? string.Empty)
		{
			this.Condition = condition;
			this.ConditionValue = conditionValue;
		}

		#endregion

		#region Properties

		[ScriptIgnore]
		public string Condition { get; protected set; }

		[ScriptIgnore]
		public new string ControlID
		{
			get { return base.ControlID; }
			set { base.ControlID = value; }
		}

		[ScriptIgnore]
		public new string Value
		{
			get { return base.Value; }
			set { base.Value = value; }
		}

		[ScriptIgnore]
		public object ConditionValue { get; set; }

		public string JavaScript
		{
			get { return this.ToString(); }
		}

		#endregion

		#region Overrides

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(GetCompareJavaScriptToken()))
			{
				if (ConditionValue is string)
				{
					return $"'{ConditionValue}'.localeCompare(GetControlValue($('{ControlID}')), undefined, {{ sensitivity: \'base\' }}){GetCompareJavaScriptToken()}0";
				}
				else if (ConditionValue is int || ConditionValue is double)
				{
					return $"GetControlNumericValue($('{ControlID}')){GetCompareJavaScriptToken()}{ConditionValue}";
				}
			}
			return string.Empty;
		}

		protected virtual string GetCompareJavaScriptToken()
		{
			return string.Empty;
		}

		#endregion
	}

	#endregion
}
