using System;
using CargoWise.Common;

namespace Enterprise.eHubMessaging.Business
{
	class LogicalExpr : Query
	{
		readonly Query opnd1;
		readonly Query opnd2;
		internal Operator.Op op;

		internal LogicalExpr(Operator.Op op, Query opnd1, Query opnd2)
		{
			this.opnd1 = opnd1;
			this.opnd2 = opnd2;
			this.op = op;
		}

		bool CompareAsNumber(XPathReader reader)
		{
			double n1 = 0, n2 = 0;
			object opndVar1, opndVar2;

			opndVar1 = this.opnd1.GetValue(reader);
			opndVar2 = this.opnd2.GetValue(reader);

			try
			{
				n1 = Convert.ToDouble(opndVar1);
				n2 = Convert.ToDouble(opndVar2);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				return false;
			}

			switch (op)
			{
				case Operator.Op.LT:
					if (n1 < n2)
					{
						return true;
					}

					break;
				case Operator.Op.GT:
					if (n1 > n2)
					{
						return true;
					}

					break;
				case Operator.Op.LE:
					if (n1 <= n2)
					{
						return true;
					}

					break;
				case Operator.Op.GE:
					if (n1 >= n2)
					{
						return true;
					}

					break;
				case Operator.Op.EQ:
					if (n1 == n2)
					{
						return true;
					}

					break;
				case Operator.Op.NE:
					if (n1 != n2)
					{
						return true;
					}

					break;
			}

			return false;
		}

		bool CompareAsString(XPathReader reader)
		{
			bool ret = false;
			object opndVar1, opndVar2;

			opndVar1 = this.opnd1.GetValue(reader);
			opndVar2 = this.opnd2.GetValue(reader);
			string s1 = null, s2 = null;
			if (opndVar1 == null || opndVar2 == null)
			{
				return false;
			}
			s1 = opndVar1.ToString();
			s2 = opndVar2.ToString();

			if (op > Operator.Op.GE)
			{
				if ((Operator.Op.EQ == op && s1.Equals(s2)) || (Operator.Op.NE == op && !s2.Equals(s2)))
				{
					ret = true;
				}
			}
			else
			{
				double n1 = 0, n2 = 0;
				try
				{
					n1 = Convert.ToDouble(s1);
					n2 = Convert.ToDouble(s2);
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
				}

				switch (op)
				{
					case Operator.Op.LT: if (n1 < n2)
						{
							ret = true;
						}

						break;
					case Operator.Op.GT: if (n1 > n2)
						{
							ret = true;
						}

						break;
					case Operator.Op.LE: if (n1 <= n2)
						{
							ret = true;
						}

						break;
					case Operator.Op.GE: if (n1 >= n2)
						{
							ret = true;
						}

						break;
				}
			}

			return ret;
		}

		bool CompareAsBoolean(XPathReader reader)
		{
			object opndVar1, opndVar2;
			bool b1, b2;

			opndVar1 = this.opnd1.GetValue(reader);
			opndVar2 = this.opnd2.GetValue(reader);

			if (opnd1.ReturnType() == XPathResultType.NodeSet)
			{
				b1 = (opndVar1 != null);
			}
			else
			{
				b1 = Convert.ToBoolean(opndVar1);
			}

			if (opnd1.ReturnType() == XPathResultType.NodeSet)
			{
				b2 = (opndVar2 != null);
			}
			else
			{
				b2 = Convert.ToBoolean(opndVar2);
			}

			if (op > Operator.Op.GE)
			{
				if ((Operator.Op.EQ == op && b1 == b2) || (Operator.Op.NE == op && b1 != b2))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				double n1 = 0, n2 = 0;
				try
				{
					n1 = Convert.ToDouble(b1);
					n2 = Convert.ToDouble(b2);
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					return false;
				}

				switch (op)
				{
					case Operator.Op.LT: if (n1 < n2)
						{
							return true;
						}

						break;
					case Operator.Op.GT: if (n1 > n2)
						{
							return true;
						}

						break;
					case Operator.Op.LE: if (n1 <= n2)
						{
							return true;
						}

						break;
					case Operator.Op.GE: if (n1 >= n2)
						{
							return true;
						}

						break;
				}
			}
			return false;
		}

		internal override object GetValue(XPathReader reader)
		{
			XPathResultType type1, type2;

			type1 = this.opnd1.ReturnType();
			type2 = this.opnd2.ReturnType();

			if (type1 == XPathResultType.Boolean || type2 == XPathResultType.Boolean)
			{
				return CompareAsBoolean(reader);
			}
			else if (type1 == XPathResultType.Number || type2 == XPathResultType.Number)
			{
				return CompareAsNumber(reader);
			}
			else
			{
				return CompareAsString(reader);
			}
		}

		internal override XPathResultType ReturnType()
		{
			return XPathResultType.Boolean;
		}
	}
}
