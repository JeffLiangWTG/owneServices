using System;
using System.Diagnostics;
using System.Globalization;
using CargoWise.Common;

namespace Enterprise.eHubMessaging.Business
{
	internal sealed class XPathScanner
	{
		readonly string xpathExpr;
		int xpathExprIndex;
		LexKind kind;
		char currentChar;
		string name;
		string prefix;
		string stringValue;
		double numberValue = double.NaN;
		bool canBeFunction;

		public XPathScanner(string xpathExpr)
		{
			if (xpathExpr == null)
			{
				throw new XPathException(String.Format("'{0}' is an invalid expression.", String.Empty));
			}
			this.xpathExpr = xpathExpr;
			NextChar();
			NextLex();
		}

		public string SourceText
		{
			get { return this.xpathExpr; }
		}

		char CurerntChar
		{
			get { return currentChar; }
		}

		bool NextChar()
		{
			Debug.Assert(0 <= xpathExprIndex && xpathExprIndex <= xpathExpr.Length);
			if (xpathExprIndex < xpathExpr.Length)
			{
				currentChar = xpathExpr[xpathExprIndex++];
				return true;
			}
			else
			{
				currentChar = '\0';
				return false;
			}
		}

		public LexKind Kind
		{
			get { return this.kind; }
		}

		public string Name
		{
			get
			{
				Debug.Assert(this.kind == LexKind.Name || this.kind == LexKind.Axe);
				Debug.Assert(this.name != null);
				return this.name;
			}
		}

		public string Prefix
		{
			get
			{
				Debug.Assert(this.kind == LexKind.Name);
				Debug.Assert(this.prefix != null);
				return this.prefix;
			}
		}

		public string StringValue
		{
			get
			{
				Debug.Assert(this.kind == LexKind.String);
				Debug.Assert(this.stringValue != null);
				return this.stringValue;
			}
		}

		public double NumberValue
		{
			get
			{
				Debug.Assert(this.kind == LexKind.Number);
				Debug.Assert(this.numberValue != double.NaN);
				return this.numberValue;
			}
		}

		public bool CanBeFunction
		{
			get
			{
				Debug.Assert(this.kind == LexKind.Name);
				return this.canBeFunction;
			}
		}

		void SkipSpace()
		{
			while (XmlCharType.IsWhiteSpace(this.CurerntChar) && NextChar())
			{
			}
		}

		public bool NextLex()
		{
			SkipSpace();
			switch (this.CurerntChar)
			{
				case '\0':
					kind = LexKind.Eof;
					return false;
				case ',':
				case '@':
				case '(':
				case ')':
				case '|':
				case '*':
				case '[':
				case ']':
				case '+':
				case '-':
				case '=':
				case '#':
				case '$':
					kind = (LexKind)Convert.ToInt32(this.CurerntChar);
					NextChar();
					break;
				case '<':
					kind = LexKind.Lt;
					NextChar();
					if (this.CurerntChar == '=')
					{
						kind = LexKind.Le;
						NextChar();
					}
					break;
				case '>':
					kind = LexKind.Gt;
					NextChar();
					if (this.CurerntChar == '=')
					{
						kind = LexKind.Ge;
						NextChar();
					}
					break;
				case '!':
					kind = LexKind.Bang;
					NextChar();
					if (this.CurerntChar == '=')
					{
						kind = LexKind.Ne;
						NextChar();
					}
					break;
				case '.':
					kind = LexKind.Dot;
					NextChar();
					if (this.CurerntChar == '.')
					{
						kind = LexKind.DotDot;
						NextChar();
					}
					else if (XmlCharType.IsDigit(this.CurerntChar))
					{
						kind = LexKind.Number;
						numberValue = ScanFraction();
					}
					break;
				case '/':
					kind = LexKind.Slash;
					NextChar();
					if (this.CurerntChar == '/')
					{
						kind = LexKind.SlashSlash;
						NextChar();
					}
					break;
				case '"':
				case '\'':
					this.kind = LexKind.String;
					this.stringValue = ScanString();
					break;
				default:
					if (XmlCharType.IsDigit(this.CurerntChar))
					{
						kind = LexKind.Number;
						numberValue = ScanNumber();
					}
					else if (XmlCharType.IsStartNCNameChar(this.CurerntChar))
					{
						kind = LexKind.Name;
						this.name = ScanName();
						this.prefix = string.Empty;
						if (this.CurerntChar == ':')
						{
							NextChar();
							if (this.CurerntChar == ':')
							{   // "foo::"
								NextChar();
								kind = LexKind.Axe;
							}
							else
							{
								this.prefix = this.name;
								if (this.CurerntChar == '*')
								{
									NextChar();
									this.name = "*";
								}
								else if (XmlCharType.IsStartNCNameChar(this.CurerntChar))
								{
									this.name = ScanName();
								}
								else
								{
									throw new XPathException(String.Format("'{0}' has an invalid qualified name.", SourceText));
								}
							}
						}
						else
						{
							SkipSpace();
							if (this.CurerntChar == ':')
							{
								NextChar();
								if (this.CurerntChar == ':')
								{
									NextChar();
									kind = LexKind.Axe;
								}
								else
								{
									throw new XPathException(String.Format("'{0}' has an invalid qualified name.", SourceText));
								}
							}
						}
						SkipSpace();
						this.canBeFunction = (this.CurerntChar == '(');
					}
					else
					{
						throw new XPathException(String.Format("'{0}' has an invalid token.", SourceText));
					}
					break;
			}
			return true;
		}

		double ScanNumber()
		{
			Debug.Assert(this.CurerntChar == '.' || XmlCharType.IsDigit(this.CurerntChar));
			int start = xpathExprIndex - 1;
			int len = 0;
			while (XmlCharType.IsDigit(this.CurerntChar))
			{
				NextChar(); len++;
			}
			if (this.CurerntChar == '.')
			{
				NextChar(); len++;
				while (XmlCharType.IsDigit(this.CurerntChar))
				{
					NextChar(); len++;
				}
			}
			return ToXPathDouble(this.xpathExpr.Substring(start, len));
		}

		double ScanFraction()
		{
			Debug.Assert(XmlCharType.IsDigit(this.CurerntChar));
			int start = xpathExprIndex - 2;
			Debug.Assert(0 <= start && this.xpathExpr[start] == '.');
			int len = 1; // '.'
			while (XmlCharType.IsDigit(this.CurerntChar))
			{
				NextChar(); len++;
			}
			return ToXPathDouble(this.xpathExpr.Substring(start, len));
		}

		string ScanString()
		{
			char endChar = this.CurerntChar;
			NextChar();
			int start = xpathExprIndex - 1;
			int len = 0;
			while (this.CurerntChar != endChar)
			{
				if (!NextChar())
				{
					throw new XPathException("This is an unclosed string.");
				}
				len++;
			}
			Debug.Assert(this.CurerntChar == endChar);
			NextChar();
			return this.xpathExpr.Substring(start, len);
		}

		string ScanName()
		{
			Debug.Assert(XmlCharType.IsStartNCNameChar(this.CurerntChar));
			int start = xpathExprIndex - 1;
			int len = 0;
			while (XmlCharType.IsNCNameChar(this.CurerntChar))
			{
				NextChar(); len++;
			}
			return this.xpathExpr.Substring(start, len);
		}
		internal static Double ToXPathDouble(Object s)
		{
			try
			{
				switch (Type.GetTypeCode(s.GetType()))
				{
					case TypeCode.String:
						try
						{
							string str = ((string)s).TrimStart();
							if (str[0] != '+')
							{
								return Double.Parse(str, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite, NumberFormatInfo.InvariantInfo);
							}
						}
						catch (Exception exception) when (!exception.IsCriticalException())
						{
						}
						return Double.NaN;
					case TypeCode.Double:
						return (double)s;
					case TypeCode.Boolean:
						return (bool)s ? 1.0 : 0.0;
					default:
						// Script functions can fead us with Int32 & Co.
						return Convert.ToDouble(s, NumberFormatInfo.InvariantInfo);
				}
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
			}
			return Double.NaN;
		}

		public enum LexKind
		{
			Comma = ',',
			Slash = '/',
			At = '@',
			Dot = '.',
			LParens = '(',
			RParens = ')',
			LBracket = '[',
			RBracket = ']',
			Star = '*',
			Plus = '+',
			Minus = '-',
			Eq = '=',
			Lt = '<',
			Gt = '>',
			Bang = '!',
			Dollar = '$',
			Apos = '\'',
			Quote = '"',
			Union = '|',
			Ne = 'N',   // !=
			Le = 'L',   // <=
			Ge = 'G',   // >=
			And = 'A',   // &&
			Or = 'O',   // ||
			DotDot = 'D',   // ..
			SlashSlash = 'S',   // //
			Name = 'n',   // XML _Name
			String = 's',   // Quoted string constant
			Number = 'd',   // _Number constant
			Axe = 'a',   // Axe (like child::)
			Eof = 'E',
		}
	}
}
