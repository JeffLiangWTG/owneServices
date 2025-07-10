namespace Enterprise.Messaging.Business.AWB
{
	#region Special Characters

	public static class SpecialChars
	{
		public const string Slant = "/";
		public const string Hyphen = "-";
		public const string CRLF = "\r\n";
	}

	#endregion

	#region Status Types

	public enum StatusType
	{
		Mandatory,
		Conditional,
		Optional
	}

	#endregion

	public class Element
	{
		public Element(StatusType status)
		{
			this.Status = status;
		}

		public bool IsMandatory
		{
			get { return Status == StatusType.Mandatory; }
		}

		public bool IsOptional
		{
			get { return Status == StatusType.Optional; }
		}

		public bool IsConditional
		{
			get { return Status == StatusType.Conditional; }
		}

		public readonly StatusType Status;
	}
}
