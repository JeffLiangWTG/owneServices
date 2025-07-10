using System;

namespace Enterprise.eHubMessaging.Business
{
	class NumericExpr : Query
	{
		internal Query opnd1;
		internal Query opnd2;
		internal Operator.Op op;

		internal NumericExpr(Operator.Op op, Query opnd1, Query opnd2)
		{
			if (opnd1.ReturnType() != XPathResultType.Number)
			{
				this.opnd1 = new NumberFunctions(opnd1);
			}
			else
			{
				this.opnd1 = opnd1;
			}

			if (opnd2 != null && (opnd2.ReturnType() != XPathResultType.Number))
			{
				this.opnd2 = new NumberFunctions(opnd2);
			}
			else
			{
				this.opnd2 = opnd2;
			}

			this.op = op;
		}

		internal override object GetValue(XPathReader reader)
		{
			double n1 = 0, n2 = 0;

			n1 = Convert.ToDouble(this.opnd1.GetValue(reader));

			if (this.op != Operator.Op.NEGATE)
			{
				n2 = Convert.ToDouble(this.opnd2.GetValue(reader));
			}

			switch (this.op)
			{
				case Operator.Op.PLUS: return n1 + n2;
				case Operator.Op.MINUS: return n1 - n2;
				case Operator.Op.MOD: return n1 % n2;
				case Operator.Op.DIV: return n1 / n2;
				case Operator.Op.MUL: return n1 * n2;
				case Operator.Op.NEGATE: return -n1;
			}

			return null;
		}

		internal override XPathResultType ReturnType()
		{
			return XPathResultType.Number;
		}
	}
}
