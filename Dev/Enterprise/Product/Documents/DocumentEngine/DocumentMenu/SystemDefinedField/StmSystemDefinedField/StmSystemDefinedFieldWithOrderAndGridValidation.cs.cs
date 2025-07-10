namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldWithOrderAndGridValidation : StmSystemDefinedFieldBaseValidation
	{
		public StmSystemDefinedFieldWithOrderAndGridValidation(StmSystemDefinedField parent) : base(parent)
		{
		}

		protected new StmSystemDefinedField Parent
		{
			get { return (StmSystemDefinedField)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			Parent.ClearRowNotifications();
			ValidateGridHasColumns();
		}

		protected override void CheckS1_Order()
		{
			base.CheckS1_Order();
			CheckOrder(Parent.S1_OrderInfo);
		}

		void ValidateGridHasColumns()
		{
			if (Parent.IsGrid && Parent.FieldColumns.Count == 0)
			{
				Parent.AddRowError(Res.GetString("f633c525-df81-4b8b-8fe0-f7ec7bff4ca7", "There are no Columns defined for this Grid."));
			}
		}
	}
}
