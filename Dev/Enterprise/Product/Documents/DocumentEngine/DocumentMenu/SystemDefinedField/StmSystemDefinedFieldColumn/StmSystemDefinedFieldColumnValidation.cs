namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldColumnValidation : StmSystemDefinedFieldBaseValidation
	{
		public StmSystemDefinedFieldColumnValidation(StmSystemDefinedFieldColumn parent) : base(parent)
		{
		}

		protected override void CheckS1_OrderColumn()
		{
			base.CheckS1_OrderColumn();
			CheckOrder(Parent.S1_OrderColumnInfo);
		}
	}
}
