namespace Enterprise.Customs.CA.Business
{
	public class B2AdjustmentsDocPage : AdjustmentsDocPage
	{
		public B2AdjustmentsDocPage() : base()
		{
		}

		protected override AdjustmentsDocPage CreateNewPage()
		{
			return new B2AdjustmentsDocPage();
		}

		protected override AdjustmentsDocLine CreateNewLine(bool isEmpty = false)
		{
			return new B2AdjustmentsDocLine(isEmpty);
		}
	}
}
