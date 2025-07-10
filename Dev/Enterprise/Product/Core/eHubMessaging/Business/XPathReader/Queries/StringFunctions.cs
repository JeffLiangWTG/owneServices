using System;
using System.Collections;
using System.Text;
using System.Xml;
using CargoWise.Common;

namespace Enterprise.eHubMessaging.Business
{
	class StringFunctions : Query
	{
		readonly ArrayList argList;
		readonly Function.FunctionType funcType;

		public StringFunctions()
		{
		}

		public StringFunctions(ArrayList qy, Function.FunctionType funcType)
		{
			this.argList = qy;
			this.funcType = funcType;
		}

		internal override object GetValue(XPathReader reader)
		{
			object obj = new object();

			switch (this.funcType)
			{
				case Function.FunctionType.FuncString:
					obj = toString(reader);
					break;

				case Function.FunctionType.FuncConcat:
					obj = Concat(reader);
					break;

				case Function.FunctionType.FuncStartsWith:
					obj = Startswith(reader);
					break;

				case Function.FunctionType.FuncContains:
					obj = Contains(reader);
					break;

				case Function.FunctionType.FuncSubstringBefore:
					obj = Substringbefore(reader);
					break;

				case Function.FunctionType.FuncSubstringAfter:
					obj = Substringafter(reader);
					break;

				case Function.FunctionType.FuncSubstring:
					obj = Substring(reader);
					break;

				case Function.FunctionType.FuncStringLength:
					obj = StringLength(reader);
					break;

				case Function.FunctionType.FuncNormalize:
					obj = Normalize(reader);
					break;

				case Function.FunctionType.FuncTranslate:
					obj = Translate(reader);
					break;
			}

			return obj;
		}

		internal override XPathResultType ReturnType()
		{
			if (this.funcType == Function.FunctionType.FuncStringLength)
			{
				return XPathResultType.Number;
			}

			if (this.funcType == Function.FunctionType.FuncStartsWith ||
				this.funcType == Function.FunctionType.FuncContains)
			{
				return XPathResultType.Boolean;
			}

			return XPathResultType.String;
		}

		String toString(XPathReader reader)
		{
			if (this.argList != null && this.argList.Count > 0)
			{
				var query = (Query)this.argList[0];

				object obj = query.GetValue(reader);

				if (obj == null)
				{
					return String.Empty;
				}

				return obj.ToString();
			}
			return String.Empty;
		}

		String Concat(XPathReader reader)
		{
			int count = 0;
			StringBuilder s = new StringBuilder();
			while (count < this.argList.Count)
			{
				s.Append(((Query)this.argList[count++]).GetValue(reader).ToString());
			}

			return s.ToString();
		}

		Boolean Startswith(XPathReader reader)
		{
			String str1 = ((Query)this.argList[0]).GetValue(reader).ToString();
			String str2 = ((Query)this.argList[1]).GetValue(reader).ToString();

			return str1.StartsWith(str2);
		}

		Boolean Contains(XPathReader reader)
		{
			String str1 = ((Query)this.argList[0]).GetValue(reader).ToString();
			String str2 = ((Query)this.argList[1]).GetValue(reader).ToString();
			int index = str1.IndexOf(str2);
			if (index != -1)
			{
				return true;
			}

			return false;
		}

		String Substringbefore(XPathReader reader)
		{
			String str1 = ((Query)this.argList[0]).GetValue(reader).ToString();
			String str2 = ((Query)this.argList[1]).GetValue(reader).ToString();
			int index = str1.IndexOf(str2);
			if (index != -1)
			{
				return str1.Substring(0, index);
			}
			else
			{
				return String.Empty;
			}
		}

		String Substringafter(XPathReader reader)
		{
			String str1 = ((Query)this.argList[0]).GetValue(reader).ToString();
			String str2 = ((Query)this.argList[1]).GetValue(reader).ToString();
			int index = str1.IndexOf(str2);
			if (index != -1)
			{
				return str1.Substring(index + str2.Length);
			}
			else
			{
				return String.Empty;
			}
		}

		String Substring(XPathReader reader)
		{
			String str1 = ((Query)this.argList[0]).GetValue(reader).ToString();
			double num = Math.Round(Convert.ToDouble(((Query)this.argList[1]).GetValue(reader))) - 1;
			if (double.IsNaN(num))
			{
				return String.Empty;
			}

			if (this.argList.Count == 3)
			{
				double num1 = Math.Round(Convert.ToDouble(((Query)this.argList[2]).GetValue(reader)));
				if (double.IsNaN(num1))
				{
					return String.Empty;
				}

				if (num < 0)
				{
					num1 = num + num1;
					if (num1 <= 0)
					{
						return String.Empty;
					}

					num = 0;
				}
				double maxlength = str1.Length - num;
				if (num1 > maxlength)
				{
					num1 = maxlength;
				}

				return str1.Substring((int)num, (int)num1);
			}
			if (num < 0)
			{
				num = 0;
			}

			return str1.Substring((int)num);
		}

		Double StringLength(XPathReader reader)
		{
			if (this.argList != null && this.argList.Count > 0)
			{
				return ((Query)this.argList[0]).GetValue(reader).ToString().Length;
			}
			return 0;
		}

		String Normalize(XPathReader reader)
		{
			String str1;
			if (this.argList != null && this.argList.Count > 0)
			{
				str1 = ((Query)this.argList[0]).GetValue(reader).ToString().Trim();
			}
			else
			{
				str1 = String.Empty;
			}

			int count = 0;
			StringBuilder str2 = new StringBuilder();
			bool firstSpace = true;
			while (count < str1.Length)
			{
				if (!XmlCharType.IsWhiteSpace(str1[count]))
				{
					firstSpace = true;
					str2.Append(str1[count]);
				}
				else
					if (firstSpace)
					{
						firstSpace = false;
						str2.Append(str1[count]);
					}
				count++;
			}
			return str2.ToString();
		}

		String Translate(XPathReader reader)
		{
			String str1 = ((Query)this.argList[0]).GetValue(reader).ToString();
			String str2 = ((Query)this.argList[1]).GetValue(reader).ToString();
			String str3 = ((Query)this.argList[2]).GetValue(reader).ToString();
			StringBuilder str = new StringBuilder();
			int count = 0, index;
			while (count < str1.Length)
			{
				index = str2.IndexOf(str1[count]);
				if (index != -1)
				{
					if (index < str3.Length)
					{
						str.Append(str3[index]);
					}
				}
				else
				{
					str.Append(str1[count]);
				}

				count++;
			}
			return str.ToString();
		}
	}

	internal sealed class NumberFunctions : Query
	{
		readonly Query _qy;
		readonly Function.FunctionType _FuncType;

		public NumberFunctions(Query qy, Function.FunctionType ftype)
		{
			_qy = qy;
			_FuncType = ftype;
		}

		public NumberFunctions()
		{
		}

		public NumberFunctions(Query qy)
		{
			_qy = qy;
			_FuncType = Function.FunctionType.FuncNumber;
		}

		internal override object GetValue(XPathReader reader)
		{
			object obj = new object();
			switch (_FuncType)
			{
				case Function.FunctionType.FuncNumber:
					obj = Number(reader);
					break;

				case Function.FunctionType.FuncFloor:
					obj = Floor(reader);
					break;

				case Function.FunctionType.FuncCeiling:
					obj = Ceiling(reader);
					break;

				case Function.FunctionType.FuncRound:
					obj = Round(reader);
					break;
			}

			return obj;
		}

		internal override XPathResultType ReturnType()
		{
			return XPathResultType.Number;
		}

		internal static double Number(bool qy)
		{
			return Convert.ToInt32(qy);
		}

		internal static double Number(String qy)
		{
			try
			{
				return Convert.ToDouble(qy);
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				return double.NaN;
			}
		}

		internal static double Number(double num)
		{
			return num;
		}

		double Number(XPathReader reader)
		{
			if (_qy != null)
			{
				object obj = _qy.GetValue(reader);

				if (obj == null)
				{
					return double.NaN;
				}

				return (Convert.ToDouble(obj));
			}

			return double.NaN;
		}

		double Floor(XPathReader reader)
		{
			return Math.Floor(Convert.ToDouble(_qy.GetValue(reader)));
		}

		double Ceiling(XPathReader reader)
		{
			return Math.Ceiling(Convert.ToDouble(_qy.GetValue(reader)));
		}

		double Round(XPathReader reader)
		{
			double n = Convert.ToDouble(_qy.GetValue(reader));
			return (-0.5 <= n && n <= 0.0) ? Math.Round(n) : Math.Floor(n + 0.5);
		}
	}

	internal sealed class BooleanFunctions : Query
	{
		readonly Query _qy;
		readonly Function.FunctionType _FuncType;

		internal BooleanFunctions(Query qy, Function.FunctionType ftype)
		{
			_qy = qy;
			_FuncType = ftype;
		}

		internal BooleanFunctions(Query qy)
		{
			_qy = qy;
			_FuncType = Function.FunctionType.FuncBoolean;
		}

		internal override object GetValue(XPathReader reader)
		{
			object obj = new object();

			switch (_FuncType)
			{
				case Function.FunctionType.FuncBoolean:
					obj = toBoolean(reader);
					break;

				case Function.FunctionType.FuncNot:
					obj = Not(reader);
					break;

				case Function.FunctionType.FuncTrue:
					obj = true;
					break;

				case Function.FunctionType.FuncFalse:
					obj = false;
					break;

				case Function.FunctionType.FuncLang:
					obj = Lang(reader);
					break;
			}
			return obj;
		}

		internal override XPathResultType ReturnType()
		{
			return XPathResultType.Boolean;
		}

		internal static Boolean toBoolean(double number)
		{
			if (number == 0 || double.IsNaN(number))
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		internal static Boolean toBoolean(String str)
		{
			if (str.Length > 0)
			{
				return true;
			}

			return false;
		}

		internal Boolean toBoolean(XPathReader reader)
		{
			Boolean ret = true;

			object obj = _qy.GetValue(reader);

			if (obj is System.Double)
			{
				double number = Convert.ToDouble(obj);
				if (number == 0 || number == double.NaN)
				{
					ret = false;
				}
			}
			else if (obj is System.String)
			{
				if (obj.ToString().Length == 0)
				{
					ret = false;
				}
			}
			else if (obj is System.Boolean)
			{
				ret = Convert.ToBoolean(obj);
			}
			else if (obj == null && reader.NodeType != XmlNodeType.EndElement)
			{
				ret = false;
			}

			return ret;
		}

		Boolean Not(XPathReader reader)
		{
			Boolean ret = toBoolean(reader);
			return !ret;
		}

		Boolean Lang(XPathReader reader)
		{
			String str = _qy.GetValue(reader).ToString();
			String lang = reader.XmlLang.ToLower();
			return (lang.Equals(str) || str.Equals(lang.Split('-')[0]));
		}
	}
}
