namespace Enterprise.Customs.KR.Business
{
	public class IDInList
	{
		public string IDType;
		public string IDValue;

		public override bool Equals(object obj)
		{
			var objectAsID = obj as IDInList;
			return objectAsID != null && objectAsID.IDType == IDType && objectAsID.IDValue == IDValue;
		}

		public override int GetHashCode()
		{
			return IDType.GetHashCode() ^ IDValue.GetHashCode();
		}
	}
}
