namespace Enterprise.Messaging.Business.AWB
{
	public class HeaderElement : Element
	{
		public HeaderElement(StatusType status, ElementList elements)
			: base(status)
		{
			this.Elements = elements;
		}

		public override string ToString()
		{
			string result = "";
			if (IsMandatory || IsConditional || (IsOptional && !string.IsNullOrEmpty(Elements.ToStringValueTypes())))
			{
				result = Elements.ToString();
			}

			return result;
		}

		public readonly ElementList Elements;
	}
}
