parser grammar LegacyMacroParser;

options { tokenVocab=LegacyMacroLexer; }

compilationUnit
    : expr=expression EOF
    ;

expression
    : number #NumberPrimaryExpr
    | literal #LiteralExpr
    | text #TextPrimaryExpr
    | NULL #NullPrimaryExpr
    | target=expression FULLSTOP call=methodCall #MethodCallExpr
    | op=NOT expr=expression #UnaryExpr
    | LPAREN expression RPAREN #BracketExpr
    | left=expression op=(DIVIDE | MULTIPLY) right=expression #BinaryExpr
    | left=expression op=(PLUS | MINUS) right=expression #BinaryExpr
    | left=expression op=(GREATERTHAN | HTMLGREATERTHAN | LESSTHAN | HTMLLESSTHAN | GREATERTHANOREQUALS | HTMLGREATERTHANOREQUALS | LESSTHANOREQUALS | HTMLLESSTHANOREQUALS) right=expression #BinaryExpr
    | left=expression op=(LOGICALEQUALS | STRICTLOGICALEQUALS | LOGICALNOTEQUALS | STRICTLOGICALNOTEQUALS) right=expression #BinaryExpr
    | left=expression op=LOGICALAND right=expression #BinaryExpr
    | left=expression op=LOGICALOR right=expression #BinaryExpr
    ;

literal
    : TRUE
    | FALSE
    ; 

number
    : sign=MINUS? num=NUMBER
    ;

text
    : DOUBLEQUOTE ~DOUBLEQUOTE*? DOUBLEQUOTE
    | SINGLEQUOTE ~SINGLEQUOTE*? SINGLEQUOTE
    ;

methodCall
    : substr
    | indexOf
    | contains
    | toLower
    | startsWith
    | endsWith
    ;

substr
    : SUBSTR LPAREN length=number RPAREN
    | SUBSTR LPAREN startIndex=number COMMA length=number RPAREN
    ;

indexOf
    : INDEXOF LPAREN textToSearchFor=text RPAREN
    | INDEXOF LPAREN textToSearchFor=text COMMA startIndex=number RPAREN
    ;

contains
    : CONTAINS LPAREN textToSearchFor=text RPAREN
    | CONTAINS LPAREN textToSearchFor=text COMMA startIndex=number RPAREN
    ;

toLower
    : TOLOWER LPAREN RPAREN
    ;

startsWith
    : STARTSWITH LPAREN textToSearchFor=text RPAREN
    ;

endsWith
    : ENDSWITH LPAREN textToSearchFor=text RPAREN
    ;