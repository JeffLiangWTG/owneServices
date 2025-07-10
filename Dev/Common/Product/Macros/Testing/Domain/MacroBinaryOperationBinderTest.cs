using CargoWise.Types;

namespace CargoWise.Macros.Testing
{
	class MacroBinaryOperationBinderTest : TestCaseWithMacros
	{
		public void TestOperationWithTwoZTypes()
		{
			const string macro = "ZDecimal + ZInteger";

			var macroRun = new MacroRun
			{
				Data = new Dummy
				{
					ZInteger = 3,
					ZDecimal = 4.2m
				},
				ExpectedResult = 7.2m
			};

			AssertMacroRun(macro, macroRun);
		}

		public void TestOperationWithNullableZTypeLeftOperand()
		{
			const string macro = "NZInteger == Integer";

			var macroRun = new MacroRun
			{
				Data = new Dummy
				{
					NZInteger = (ZInt?)3,
					Integer = 3
				},
				ExpectedResult = true
			};

			AssertMacroRun(macro, macroRun);
		}

		public void TestOperationWithNullableZTypeRightOperand()
		{
			const string macro = "Integer == NZInteger";

			var macroRun = new MacroRun
			{
				Data = new Dummy
				{
					NZInteger = (ZInt?)3,
					Integer = 3
				},
				ExpectedResult = true
			};

			AssertMacroRun(macro, macroRun);
		}

		#region Implementation

		class Dummy
		{
			public bool Bool { get; set; }
			public int Integer { get; set; }
			public ZInt ZInteger { get; set; }
			public ZInt? NZInteger { get; set; }
			public ZDecimal ZDecimal { get; set; }

			public static bool operator ==(Dummy left, Dummy right)
			{
				return left.Integer == right.Integer;
			}

			public static bool operator !=(Dummy left, Dummy right)
			{
				return left.Integer != right.Integer;
			}

			public static bool operator &(Dummy left, Dummy right)
			{
				return left.Bool && right.Bool;
			}

			public static bool operator |(Dummy left, Dummy right)
			{
				return left.Bool || right.Bool;
			}

			public static int operator +(Dummy left, Dummy right)
			{
				return left.Integer + right.Integer;
			}

			public static int operator -(Dummy left, Dummy right)
			{
				return left.Integer - right.Integer;
			}

			public static int operator *(Dummy left, Dummy right)
			{
				return left.Integer * right.Integer;
			}

			public static int operator /(Dummy left, Dummy right)
			{
				return left.Integer / right.Integer;
			}

			public static bool operator >(Dummy left, Dummy right)
			{
				return left.Integer > right.Integer;
			}

			public static bool operator <(Dummy left, Dummy right)
			{
				return left.Integer < right.Integer;
			}

			public static bool operator >=(Dummy left, Dummy right)
			{
				return left.Integer >= right.Integer;
			}

			public static bool operator <=(Dummy left, Dummy right)
			{
				return left.Integer <= right.Integer;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0660 error")]
			public override bool Equals(object obj)
			{
				return base.Equals(obj);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Decruftification", "WTG3007:Overrides should not simply call base.", Justification = "Required to avoid CS0661 error")]
			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		#endregion
	}
}
