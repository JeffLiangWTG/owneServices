using System.CodeDom;

namespace Enterprise.ZArchitecture.GUI
{
	internal abstract class CodeDomVisitor
	{
		#region VisitMember

		void VisitMemberCollection(CodeTypeMemberCollection collection)
		{
			foreach (CodeTypeMember member in collection)
			{
				VisitMember(member);
			}
		}

		protected virtual void VisitMember(CodeTypeMember member)
		{
			VisitDirectiveCollection(member.StartDirectives);
			VisitDirectiveCollection(member.EndDirectives);
			VisitCustomAttributeCollection(member.CustomAttributes);
			VisitCommentCollection(member.Comments);

			var ev = member as CodeMemberEvent;
			var field = member as CodeMemberField;
			var method = member as CodeMemberMethod;
			var property = member as CodeMemberProperty;
			var snippet = member as CodeSnippetTypeMember;
			var type = member as CodeTypeDeclaration;
			var d = member as CodeTypeDelegate;
			if (ev != null)
			{
				VisitTypeReferenceCollection(ev.ImplementationTypes);
				VisitTypeReference(ev.PrivateImplementationType);
				VisitTypeReference(ev.Type);
			}
			if (field != null)
			{
				VisitExpression(field.InitExpression);
				VisitTypeReference(field.Type);
			}
			if (method != null)
			{
				VisitTypeReferenceCollection(method.ImplementationTypes);
				VisitParameterDeclarationExpressionCollection(method.Parameters);
				VisitTypeReference(method.PrivateImplementationType);
				VisitTypeReference(method.ReturnType);
				VisitCustomAttributeCollection(method.ReturnTypeCustomAttributes);
				VisitStatementCollection(method.Statements);
				VisitTypeReferenceCollection(method.TypeParameters);
			}
			if (property != null)
			{
				VisitStatementCollection(property.GetStatements);
				VisitStatementCollection(property.SetStatements);
				VisitTypeReferenceCollection(property.ImplementationTypes);
				VisitTypeReference(property.PrivateImplementationType);
				VisitParameterDeclarationExpressionCollection(property.Parameters);
			}
			if (type != null)
			{
				VisitTypeReferenceCollection(type.BaseTypes);
				VisitTypeParameters(type.TypeParameters);
				VisitMemberCollection(type.Members);
			}
			if (d != null)
			{
				VisitParameterDeclarationExpressionCollection(d.Parameters);
				VisitTypeReference(d.ReturnType);
			}
		}

		#endregion

		#region VisitStatement

		void VisitStatementCollection(CodeStatementCollection collection)
		{
			foreach (CodeStatement statement in collection)
			{
				VisitStatement(statement);
			}
		}

		protected virtual void VisitStatement(CodeStatement statement)
		{
			VisitDirectiveCollection(statement.StartDirectives);
			VisitDirectiveCollection(statement.EndDirectives);

			var assign = statement as CodeAssignStatement;
			var attachEvent = statement as CodeAttachEventStatement;
			var condition = statement as CodeConditionStatement;
			var expr = statement as CodeExpressionStatement;
			var gotoStatement = statement as CodeGotoStatement;
			var iteration = statement as CodeIterationStatement;
			var label = statement as CodeLabeledStatement;
			var methodReturn = statement as CodeMethodReturnStatement;
			var removeEvent = statement as CodeRemoveEventStatement;
			var throwException = statement as CodeThrowExceptionStatement;
			var tryCatch = statement as CodeTryCatchFinallyStatement;
			var variable = statement as CodeVariableDeclarationStatement;
			if (assign != null)
			{
				VisitExpression(assign.Left);
				VisitExpression(assign.Right);
			}
			if (attachEvent != null)
			{
				VisitExpression(attachEvent.Event);
				VisitExpression(attachEvent.Listener);
			}
			if (removeEvent != null)
			{
				VisitExpression(removeEvent.Event);
				VisitExpression(removeEvent.Listener);
			}
			if (condition != null)
			{
				VisitExpression(condition.Condition);
				VisitStatementCollection(condition.TrueStatements);
				VisitStatementCollection(condition.FalseStatements);
			}
			if (expr != null)
			{
				VisitExpression(expr.Expression);
			}
			if (iteration != null)
			{
				VisitStatement(iteration.IncrementStatement);
				VisitStatement(iteration.InitStatement);
				VisitExpression(iteration.TestExpression);
				VisitStatementCollection(iteration.Statements);
			}
			if (label != null)
			{
				VisitStatement(label.Statement);
			}
			if (methodReturn != null)
			{
				VisitExpression(methodReturn.Expression);
			}
			if (throwException != null)
			{
				VisitExpression(throwException.ToThrow);
			}
			if (tryCatch != null)
			{
				VisitStatementCollection(tryCatch.TryStatements);
				VisitCatchClauseCollection(tryCatch.CatchClauses);
				VisitStatementCollection(tryCatch.FinallyStatements);
			}
			if (variable != null)
			{
				VisitExpression(variable.InitExpression);
			}
		}

		void VisitCatchClauseCollection(CodeCatchClauseCollection collection)
		{
			foreach (CodeCatchClause catchClause in collection)
			{
				VisitCatchClause(catchClause);
			}
		}

		#endregion

		#region VisitExpression

		void VisitExpressionCollection(CodeExpressionCollection collection)
		{
			foreach (CodeExpression expr in collection)
			{
				VisitExpression(expr);
			}
		}

		protected virtual void VisitExpression(CodeExpression expr)
		{
			var argument = expr as CodeArgumentReferenceExpression;
			var arrayCreate = expr as CodeArrayCreateExpression;
			var arrayIndexer = expr as CodeArrayIndexerExpression;
			var baseReference = expr as CodeBaseReferenceExpression;
			var binaryOperator = expr as CodeBinaryOperatorExpression;
			var codeCast = expr as CodeCastExpression;
			var defaultValue = expr as CodeDefaultValueExpression;
			var delegateCreate = expr as CodeDelegateCreateExpression;
			var delegateInvoke = expr as CodeDelegateInvokeExpression;
			var direction = expr as CodeDirectionExpression;
			var eventReference = expr as CodeEventReferenceExpression;
			var fieldReference = expr as CodeFieldReferenceExpression;
			var indexer = expr as CodeIndexerExpression;
			var methodInvoke = expr as CodeMethodInvokeExpression;
			var methodReference = expr as CodeMethodReferenceExpression;
			var objectCreate = expr as CodeObjectCreateExpression;
			var parameterDeclaration = expr as CodeParameterDeclarationExpression;
			var primitiveExpression = expr as CodePrimitiveExpression;
			var propertyReference = expr as CodePropertyReferenceExpression;
			var setValueReference = expr as CodePropertySetValueReferenceExpression;
			var snippet = expr as CodeSnippetExpression;
			var thisReference = expr as CodeThisReferenceExpression;
			var typeOf = expr as CodeTypeOfExpression;
			var typeReference = expr as CodeTypeReferenceExpression;
			var variableReference = expr as CodeVariableReferenceExpression;
			if (arrayCreate != null)
			{
				VisitTypeReference(arrayCreate.CreateType);
				VisitExpressionCollection(arrayCreate.Initializers);
				VisitExpression(arrayCreate.SizeExpression);
			}
			if (arrayIndexer != null)
			{
				VisitExpressionCollection(arrayIndexer.Indices);
				VisitExpression(arrayIndexer.TargetObject);
			}
			if (binaryOperator != null)
			{
				VisitExpression(binaryOperator.Left);
				VisitExpression(binaryOperator.Right);
			}
			if (codeCast != null)
			{
				VisitExpression(codeCast.Expression);
				VisitTypeReference(codeCast.TargetType);
			}
			if (defaultValue != null)
			{
				VisitTypeReference(defaultValue.Type);
			}
			if (delegateCreate != null)
			{
				VisitTypeReference(delegateCreate.DelegateType);
				VisitExpression(delegateCreate.TargetObject);
			}
			if (delegateInvoke != null)
			{
				VisitExpressionCollection(delegateInvoke.Parameters);
				VisitExpression(delegateInvoke.TargetObject);
			}
			if (direction != null)
			{
				VisitExpression(direction.Expression);
			}
			if (eventReference != null)
			{
				VisitExpression(eventReference.TargetObject);
			}
			if (fieldReference != null)
			{
				VisitExpression(fieldReference.TargetObject);
			}
			if (indexer != null)
			{
				VisitExpressionCollection(indexer.Indices);
				VisitExpression(indexer.TargetObject);
			}
			if (methodInvoke != null)
			{
				VisitExpressionCollection(methodInvoke.Parameters);
			}
			if (methodReference != null)
			{
				VisitExpression(methodReference.TargetObject);
				VisitTypeReferenceCollection(methodReference.TypeArguments);
			}
			if (objectCreate != null)
			{
				VisitTypeReference(objectCreate.CreateType);
				VisitExpressionCollection(objectCreate.Parameters);
			}
			if (parameterDeclaration != null)
			{
				VisitCustomAttributeCollection(parameterDeclaration.CustomAttributes);
			}
			if (propertyReference != null)
			{
				VisitExpression(propertyReference.TargetObject);
			}
			if (typeOf != null)
			{
				VisitTypeReference(typeOf.Type);
			}
			if (typeReference != null)
			{
				VisitTypeReference(typeReference.Type);
			}
		}

		#endregion

		#region Other

		protected virtual void VisitTypeReference(CodeTypeReference reference)
		{
		}

		protected virtual void VisitComment(CodeComment comment)
		{
		}

		protected virtual void VisitCustomAttribute(CodeAttributeDeclaration attr)
		{
		}

		protected virtual void VisitDirective(CodeDirective directive)
		{
		}

		void VisitCatchClause(CodeCatchClause catchClause)
		{
			VisitTypeReference(catchClause.CatchExceptionType);
			VisitStatementCollection(catchClause.Statements);
		}

		void VisitTypeParameters(CodeTypeParameterCollection collection)
		{
			foreach (CodeTypeParameter parameter in collection)
			{
				VisitTypeReferenceCollection(parameter.Constraints);
				VisitCustomAttributeCollection(parameter.CustomAttributes);
			}
		}

		void VisitTypeReferenceCollection(CodeTypeParameterCollection collection)
		{
			foreach (CodeTypeReference type in collection)
			{
				VisitTypeReference(type);
			}
		}

		void VisitParameterDeclarationExpressionCollection(CodeParameterDeclarationExpressionCollection collection)
		{
			foreach (CodeParameterDeclarationExpression expr in collection)
			{
				VisitCustomAttributeCollection(expr.CustomAttributes);
			}
		}

		void VisitTypeReferenceCollection(CodeTypeReferenceCollection collection)
		{
			foreach (CodeTypeReference type in collection)
			{
				VisitTypeReference(type);
			}
		}

		void VisitCommentCollection(CodeCommentStatementCollection collection)
		{
			foreach (CodeComment comment in collection)
			{
				VisitComment(comment);
			}
		}

		void VisitCustomAttributeCollection(CodeAttributeDeclarationCollection collection)
		{
			foreach (CodeAttributeDeclaration attr in collection)
			{
				VisitCustomAttribute(attr);
			}
		}

		void VisitDirectiveCollection(CodeDirectiveCollection collection)
		{
			foreach (CodeDirective directive in collection)
			{
				VisitDirective(directive);
			}
		}

		#endregion
	}
}
