using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.IO;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	internal class XmlBuilder : ElementProcessor, IDisposable
	{
		class Transaction
		{
			public readonly List<string> Elements = new List<string>();
			public bool HasAttributes;
		}

		internal XmlBuilder(Stream outputStream, string nameSpace, string version)
			: base(nameSpace, version)
		{
			this.writer = new StreamWriter(new UnclosableStreamWrapper(outputStream));
		}

		readonly StreamWriter writer;
		int indentCount;
		int blankLineCount;
		bool lastLineIndented;
		readonly Stack<Transaction> transactions = new Stack<Transaction>();

		public bool RemoveEmptyElements { get; set; }

		void IDisposable.Dispose()
		{
			writer.Dispose();
		}

		#region SuppressResourceStringsCheckRegion

		void BeginTransaction()
		{
			transactions.Push(new Transaction());
		}

		void CommitTransaciton()
		{
			var transaction = transactions.Pop();

			if (transaction.Elements.Count <= 2 && !transaction.HasAttributes)
			{
				return;
			}

			if (transactions.Count == 0)
			{
				foreach (var element in transaction.Elements)
				{
					writer.WriteLine(element);
				}
			}
			else
			{
				transactions.Peek().Elements.AddRange(transaction.Elements);
			}
		}

		public void WriteXMLDeclaration()
		{
			writer.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>");
		}

		public void AddBlankLine()
		{
			lastLineIndented = false;
			blankLineCount++;
		}

		public void AddBlankLineIfNotFirstInIndentedSection()
		{
			if (!lastLineIndented)
			{
				AddBlankLine();
			}
		}

		public void AddStartElement(string element)
		{
			if (RemoveEmptyElements)
			{
				BeginTransaction();
				transactions.Peek().Elements.Add(GetOpenElement(element.Trim()));
				return;
			}

			WriteOpenElement(element.Trim());
			writer.WriteLine();
		}

		public void AddStartElementWithAttributes(string element, params string[] attributes)
		{
			if (RemoveEmptyElements)
			{
				BeginTransaction();
				transactions.Peek().Elements.Add(GetOpenElement(element.Trim(), attributes));

				if (attributes?.Length > 0 && attributes.Any(a => !string.IsNullOrWhiteSpace(a)))
				{
					transactions.Peek().HasAttributes = true;
				}

				return;
			}

			WriteOpenElement(element.Trim(), attributes);
			writer.WriteLine();
		}

		public void AddEndElement(string element)
		{
			if (RemoveEmptyElements)
			{
				transactions.Peek().Elements.Add(new string(' ', (indentCount - 1) * 2) + GetCloseElement(element.Trim()));
				CommitTransaciton();
				return;
			}

			writer.Write(new string(' ', (indentCount - 1) * 2));
			WriteCloseElement(element.Trim());
			writer.WriteLine();
		}

		public void AddElementWithValue(string element, string value)
		{
			var trimmedElement = element.Trim();

			if (RemoveEmptyElements)
			{
				if (!string.IsNullOrEmpty(value))
				{
					transactions.Peek().Elements.Add(GetOpenElement(trimmedElement) + GetSanitizedValueToXML(value) + GetCloseElement(trimmedElement));
				}

				return;
			}

			WriteOpenElement(trimmedElement);
			WriteSanitizedValueToXml(value);
			WriteCloseElement(trimmedElement);
			writer.WriteLine();
		}

		public void AddElementWithValueWithAttributes(string element, string value, params string[] attributes)
		{
			var trimmedElement = element.Trim();

			if (RemoveEmptyElements)
			{
				if (attributes?.Length > 0 && attributes.Any(a => !string.IsNullOrWhiteSpace(a)))
				{
					transactions.Peek().HasAttributes = true;
				}
				else if (string.IsNullOrEmpty(value))
				{
					return;
				}

				transactions.Peek().Elements.Add(GetOpenElement(trimmedElement, attributes) + GetSanitizedValueToXML(value) + GetCloseElement(trimmedElement));
				return;
			}

			WriteOpenElement(trimmedElement, attributes);
			WriteSanitizedValueToXml(value);
			WriteCloseElement(trimmedElement);
			writer.WriteLine();
		}

		void WriteOpenElement(string element, params string[] attributes)
		{
			for (; blankLineCount > 0; blankLineCount--)
			{
				writer.WriteLine();
			}

			writer.Write(new string(' ', indentCount * 2));
			indentCount++;
			lastLineIndented = true;
			writer.Write('<');
			writer.Write(element);

			foreach (var attribute in attributes.Where(a => !string.IsNullOrWhiteSpace(a)))
			{
				writer.Write(' ');
				writer.Write(attribute.Trim());
			}

			writer.Write('>');
		}

		void WriteCloseElement(string element)
		{
			indentCount--;
			lastLineIndented = true;
			blankLineCount = 0;
			writer.Write("</");
			writer.Write(element);
			writer.Write('>');
		}

		void WriteSanitizedValueToXml(string value)
		{
			foreach (var c in value)
			{
				if (XmlConvert.IsXmlChar(c))
				{
					writer.Write(c);
				}
			}
		}

		string GetOpenElement(string element, params string[] attributes)
		{
			for (; blankLineCount > 0; blankLineCount--)
			{
				transactions.Peek().Elements.Add(string.Empty);
			}

			using var result = new StringWriter();
			result.Write(new string(' ', indentCount * 2));

			indentCount++;
			lastLineIndented = true;

			result.Write('<');
			result.Write(element);

			foreach (var attribute in attributes.Where(a => !string.IsNullOrWhiteSpace(a)))
			{
				result.Write(' ');
				result.Write(attribute.Trim());
			}

			result.Write('>');

			return result.ToString();
		}

		string GetCloseElement(string element)
		{
			indentCount--;
			lastLineIndented = true;
			blankLineCount = 0;

			return $"</{element}>";
		}

		static string GetSanitizedValueToXML(string value)
		{
			using var result = new StringWriter();
			foreach (var c in value)
			{
				if (XmlConvert.IsXmlChar(c))
				{
					result.Write(c);
				}
			}

			return result.ToString();
		}

		#endregion
	}
}
