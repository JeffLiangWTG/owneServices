using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Messaging.MessageProcessors;
using Res = Enterprise.Customs.Common.Res;

namespace Enterprise.Customs.Business.MessageInterpretation
{
	public interface ISegmentInterpretation
	{
		void AddElementInterpretation(string elementDescription, object elementValue);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void AddElementInterpretation<T>(Expression<Func<T>> property);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void AddElementInterpretation<T>(Expression<Func<T>> elementDescription, object elementValue);
		void AddElementInterpretationIfNotEmpty(string elementDescription, IZType elementValue);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void AddElementInterpretationIfNotEmpty<T>(Expression<Func<T>> property) where T : IZType;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		void AddElementInterpretationIfNotEmpty<T>(Expression<Func<T>> elementDescription, IZType elementValue) where T : IZType;
	}

	public class MessageInterpretation : HtmlTableCreator
	{
		public MessageInterpretation(SegmentGroup message, UNCharacterSet characterSet)
			: base(new[]
			{
				Res.GetString("85766139-2575-4bfa-b112-7a3a44b9b3c1", "Segment"),
				Res.GetString("b886cc5f-5938-44b7-a4e5-f1bf17f4251e", "Segment/Element Description"),
				Res.GetString("10f2da73-7f3e-4ec8-8d60-e50770885be6", "Value")
			})
		{
			this.message = message;
			this.characterSet = characterSet;
			interpretations = new Dictionary<Segment, SegmentInterpretation>();
		}

		#region AddNewSegmentInterpretation

		public ISegmentInterpretation AddNewSegmentInterpretation(Segment segment)
		{
			if (interpretations.ContainsKey(segment))
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Segment '{0}' already added", segment.ToString(characterSet)));
			}

			return (interpretations[segment] = new SegmentInterpretation(segment, characterSet));
		}

		public ISegmentInterpretation AddNewSegmentInterpretation(Segment segment, string elementDescription, object elementValue)
		{
			var segmentInterpretation = AddNewSegmentInterpretation(segment);
			segmentInterpretation.AddElementInterpretation(elementDescription, elementValue);
			return segmentInterpretation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ISegmentInterpretation AddNewSegmentInterpretation<T>(Segment segment, Expression<Func<T>> elementDescription, object elementValue)
		{
			var segmentInterpretation = AddNewSegmentInterpretation(segment);
			segmentInterpretation.AddElementInterpretation(elementDescription, elementValue);
			return segmentInterpretation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public ISegmentInterpretation AddNewSegmentInterpretation<T>(Segment segment, params Expression<Func<T>>[] properties)
		{
			var segmentInterpretation = AddNewSegmentInterpretation(segment);
			foreach (var property in properties)
			{
				segmentInterpretation.AddElementInterpretation(property);
			}
			return segmentInterpretation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public string GetFriendlyPropertyName<T>(Expression<Func<T>> property)
		{
			return PropertyNameProvider.GetFriendlyPropertyName(property);
		}

		#endregion

		#region Common Interpretations

		public void AddUNHInterpretation(Segment unh, string messageNumber)
		{
			AddNewSegmentInterpretation(unh, Res.GetString("6e35f381-a01e-4909-b1b8-ae09650837ff", "Message Header"), messageNumber);
		}

		public void AddUNS1Interpretation(Segment uns)
		{
			AddNewSegmentInterpretation(uns, Res.GetString("cd86ee41-b1b4-4483-945b-5a70a0d502a9", "Headers/Details Section Separator"), string.Empty);
		}

		public void AddUNS2Interpretation(Segment uns)
		{
			AddNewSegmentInterpretation(uns, Res.GetString("604f397c-d090-4102-8c9b-6acdd073a75e", "Details/Summary Section Separator"), string.Empty);
		}

		public void AddMandatoryTriggerSegmentInterpretation(Segment segment)
		{
			AddNewSegmentInterpretation(segment, Res.GetString("c7636bcb-d09a-4851-8083-95ba1b8ef000", "Mandatory Trigger Segment"), string.Empty);
		}

		public void AddUNTInterpretation(Segment unt, string messageNumber)
		{
			AddNewSegmentInterpretation(unt, Res.GetString("3bf6cc88-8a67-4ac2-9b99-d9e088c7c0b6", "Message Trailer"), messageNumber);
		}

		#endregion

		#region ToHtml

		public new string ToHtml()
		{
			ToHtml(message);
			return "<br>" + base.ToHtml();
		}

		void ToHtml(SegmentGroup group)
		{
			foreach (IEnumerable messageSection in group.MessageSections)
			{
				foreach (var value in messageSection)
				{
					var segment = value as Segment;
					if (segment != null)
					{
						if (interpretations.ContainsKey(segment))
						{
							interpretations[segment].ToHtml(this);
						}
						else
						{
							WriteRow(segment.ToString(characterSet), string.Empty, string.Empty);
						}
					}
					else
					{
						var segmentGroup = value as SegmentGroup;

						if (segmentGroup != null)
						{
							ToHtml(segmentGroup);
						}
					}
				}
			}
		}

		#endregion

		#region SegmentInterpretation

		class SegmentInterpretation : ISegmentInterpretation
		{
			public SegmentInterpretation(Segment segment, UNCharacterSet characterSet)
			{
				this.segment = segment;
				this.characterSet = characterSet;
				interpretations = new List<KeyValuePair<string, object>>();
			}

			#region ISegmentInterpretation Members

			#region AddElementInterpretation

			public void AddElementInterpretation(string elementDescription, object elementValue)
			{
				interpretations.Add(new KeyValuePair<string, object>(elementDescription, elementValue));
			}

			public void AddElementInterpretation<T>(Expression<Func<T>> property)
			{
				AddElementInterpretation(PropertyNameProvider.GetFriendlyPropertyName(property), property.Compile()());
			}

			public void AddElementInterpretation<T>(Expression<Func<T>> elementDescription, object elementValue)
			{
				AddElementInterpretation(PropertyNameProvider.GetFriendlyPropertyName(elementDescription), elementValue);
			}

			#endregion

			#region AddElementInterpretationIfNotEmpty

			public void AddElementInterpretationIfNotEmpty(string elementDescription, IZType elementValue)
			{
				if (!elementValue.IsEmpty)
				{
					AddElementInterpretation(elementDescription, elementValue);
				}
			}

			public void AddElementInterpretationIfNotEmpty<T>(Expression<Func<T>> property) where T : IZType
			{
				AddElementInterpretationIfNotEmpty(property, property.Compile()());
			}

			public void AddElementInterpretationIfNotEmpty<T>(Expression<Func<T>> elementDescription, IZType elementValue) where T : IZType
			{
				if (!elementValue.IsEmpty)
				{
					AddElementInterpretation(PropertyNameProvider.GetFriendlyPropertyName(elementDescription), elementValue);
				}
			}

			#endregion

			#endregion

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html string")]
			public void ToHtml(HtmlTableCreator table)
			{
				var firstRow = true;
				foreach (var pair in interpretations)
				{
					if (firstRow)
					{
						var attributes = new NameValueCollection { { "style", "word-break:break-all" }, { "rowspan", interpretations.Count.ToString(CultureInfo.CurrentCulture) } };
						var segmentCell = new CellWithFormatting(segment.ToString(characterSet), attributes);
						table.WriteRowWithFormatting(segmentCell, new CellWithFormatting(pair.Key), new CellWithFormatting(ToStringSafe(pair.Value)));
						firstRow = false;
					}
					else
					{
						table.WriteRow(pair.Key, ToStringSafe(pair.Value));
					}
				}
			}

			static string ToStringSafe(object obj)
			{
				return obj == null ? string.Empty : obj.ToString();
			}

			readonly Segment segment;
			readonly UNCharacterSet characterSet;
			readonly List<KeyValuePair<string, object>> interpretations;
		}

		#endregion

		readonly SegmentGroup message;
		readonly UNCharacterSet characterSet;
		readonly Dictionary<Segment, SegmentInterpretation> interpretations;
	}
}
