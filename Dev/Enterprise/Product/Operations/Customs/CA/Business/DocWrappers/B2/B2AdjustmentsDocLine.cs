namespace Enterprise.Customs.CA.Business
{
	public class B2AdjustmentsDocLine : AdjustmentsDocLine
	{
		public B2AdjustmentsDocLine(bool isEmpty = false) : base(isEmpty)
		{
		}

		protected override AdjustmentsDocLine CreateNewLine()
		{
			return new B2AdjustmentsDocLine();
		}
	}
}
